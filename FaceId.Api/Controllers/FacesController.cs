using FaceId.Api.Data;
using FaceId.Api.Models;
using FaceId.Api.Servises;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FaceId.Api.Controllers;

[ApiController]
[Route("api/faces")]
public class FacesController : ControllerBase
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly FaceIdDbContext _db;
    private readonly FaceRecognitionClient _faceRecognitionClient;
    private readonly IWebHostEnvironment _environment;

    public FacesController(
        FaceIdDbContext db,
        FaceRecognitionClient faceRecognitionClient,
        IWebHostEnvironment environment)
    {
        _db = db;
        _faceRecognitionClient = faceRecognitionClient;
        _environment = environment;
    }

    [HttpPost("users/{userId:int}/register")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegisterFace(
        int userId,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        // 1. Проверяем наличие файла.
        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                error = "Необходимо загрузить фотографию."
            });
        }

        // 2. Проверяем размер фотографии.
        if (file.Length > MaxFileSize)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new
            {
                error = "Размер фотографии не должен превышать 5 МБ."
            });
        }

        // 3. Проверяем формат файла.
        var extension = file.ContentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => null
        };

        if (extension is null)
        {
            return BadRequest(new
            {
                error = "Поддерживаются только JPEG, PNG и WebP."
            });
        }

        // 4. Проверяем, существует ли пользователь.
        var userExists = await _db.Users
            .AnyAsync(user => user.Id == userId, cancellationToken);

        if (!userExists)
        {
            return NotFound(new
            {
                error = $"Пользователь с ID {userId} не найден."
            });
        }

        // 5. Получаем embedding через Python.
        float[] embedding;

        try
        {
            await using var imageStream = file.OpenReadStream();

            embedding = await _faceRecognitionClient.ExtractEmbeddingAsync(
                imageStream,
                file.FileName,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            // Python вернул ошибку обработки изображения.
            if (exception.StatusCode is not null)
            {
                var statusCode = (int)exception.StatusCode.Value;

                if (statusCode == StatusCodes.Status400BadRequest)
                {
                    return BadRequest(new
                    {
                        error = "Не удалось обработать изображение."
                    });
                }

                if (statusCode == StatusCodes.Status413PayloadTooLarge)
                {
                    return StatusCode(
                        StatusCodes.Status413PayloadTooLarge,
                        new { error = "Файл слишком большой." });
                }

                if (statusCode == StatusCodes.Status422UnprocessableEntity)
                {
                    return UnprocessableEntity(new
                    {
                        error = "На фотографии должно быть ровно одно лицо."
                    });
                }
            }

            // Если Python недоступен или произошла другая ошибка.
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = "Сервис распознавания лиц временно недоступен."
            });
        }

        // 6. Создаём каталог для фотографий.
        var storageDirectory = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "faces");

        Directory.CreateDirectory(storageDirectory);

        // Имя генерируется сервером, а не берётся от пользователя.
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(storageDirectory, fileName);

        // Относительный путь будем хранить в базе данных.
        var relativePath = Path.Combine("Storage", "faces", fileName)
            .Replace('\\', '/');

        // 7. Сохраняем фотографию.
        await using (var fileStream = new FileStream(
            absolutePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        try
        {
            // 8. Проверяем, зарегистрировано ли уже лицо пользователя.
            var face = await _db.Faces
                .SingleOrDefaultAsync(
                    item => item.AppUserId == userId,
                    cancellationToken);

            var now = DateTime.UtcNow;

            if (face is null)
            {
                // Первичная регистрация лица.
                face = new Face
                {
                    AppUserId = userId,
                    Embedding = embedding,
                    CreatedAt = now
                };

                _db.Faces.Add(face);
            }
            else
            {
                // Повторная регистрация: обновляем embedding.
                face.Embedding = embedding;
            }

            // 9. Сохраняем сведения о фотографии.
            var photo = new FacePhoto
            {
                PhotoPath = relativePath,
                CreatedAt = now,
                Face = face
            };

            _db.FacePhotos.Add(photo);

            // 10. Сохраняем изменения в PostgreSQL.
            await _db.SaveChangesAsync(cancellationToken);

            // 11. Возвращаем результат без самого embedding.
            return Ok(new
            {
                message = "Лицо успешно зарегистрировано.",
                userId,
                faceId = face.Id,
                photoId = photo.Id,
                embeddingDimensions = embedding.Length,
                photoPath = relativePath
            });
        }
        catch
        {
            // Если запись в БД не удалась, удаляем сохранённый файл.
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }

            throw;
        }
    }

    [HttpPost("recognize")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Recognize(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        // 1. Проверяем фотографию.
        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                error = "Необходимо загрузить фотографию."
            });
        }

        if (file.Length > MaxFileSize)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new
            {
                error = "Размер фотографии не должен превышать 5 МБ."
            });
        }

        var supportedTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        if (!supportedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new
            {
                error = "Поддерживаются только JPEG, PNG и WebP."
            });
        }

        // 2. Получаем embedding из Python.
        float[] embedding;

        try
        {
            await using var imageStream = file.OpenReadStream();

            embedding = await _faceRecognitionClient.ExtractEmbeddingAsync(
                imageStream,
                file.FileName,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            // Записываем неудачную попытку обработки фотографии.
            _db.AccessLogs.Add(new AccessLog
            {
                UserId = null,
                RecognizedAt = DateTime.UtcNow,
                Status = "ScanFailed"
            });

            await _db.SaveChangesAsync(cancellationToken);

            if (exception.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                return UnprocessableEntity(new
                {
                    recognized = false,
                    status = "ScanFailed",
                    message = "На фотографии должно быть ровно одно лицо."
                });
            }

            if (exception.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return BadRequest(new
                {
                    status = "ScanFailed",
                    message = "Не удалось обработать изображение."
                });
            }

            if ((int?)exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                return StatusCode(StatusCodes.Status413PayloadTooLarge, new
                {
                    status = "ScanFailed",
                    message = "Фотография слишком большая."
                });
            }

            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                status = "ScanFailed",
                message = "Сервис распознавания временно недоступен."
            });
        }

        // 3. Загружаем зарегистрированные лица.
        var registeredFaces = await _db.Faces
            .Include(face => face.User)
            .Where(face => face.Embedding != null)
            .ToListAsync(cancellationToken);

        // 4. Ищем наиболее похожее лицо.
        const double similarityThreshold = 0.45;

        var bestMatch = registeredFaces
            .Select(face => new
            {
                Face = face,
                Similarity = CalculateCosineSimilarity(
                    embedding,
                    face.Embedding!)
            })
            .OrderByDescending(match => match.Similarity)
            .FirstOrDefault();

        var recognized = bestMatch is not null
            && bestMatch.Similarity >= similarityThreshold;

        // 5. Записываем результат распознавания.
        var accessLog = new AccessLog
        {
            UserId = recognized ? bestMatch!.Face.AppUserId : null,
            RecognizedAt = DateTime.UtcNow,
            Status = recognized ? "Confirmed" : "NotFound"
        };

        _db.AccessLogs.Add(accessLog);

        await _db.SaveChangesAsync(cancellationToken);

        // 6. Возвращаем результат клиенту.
        if (!recognized)
        {
            return Ok(new
            {
                recognized = false,
                status = "NotFound",
                message = "Зарегистрированный пользователь не найден.",
                similarity = bestMatch is null
                    ? (double?)null
                    : Math.Round(bestMatch.Similarity, 4)
            });
        }

        var user = bestMatch!.Face.User;

        return Ok(new
        {
            recognized = true,
            status = "Confirmed",
            similarity = Math.Round(bestMatch.Similarity, 4),
            user = new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email
            }
        });
    }

    private static double CalculateCosineSimilarity(
        float[] first,
        float[] second)
    {
        if (first.Length != second.Length || first.Length == 0)
        {
            return -1;
        }

        double dotProduct = 0;
        double firstNorm = 0;
        double secondNorm = 0;

        for (var i = 0; i < first.Length; i++)
        {
            dotProduct += (double)first[i] * second[i];
            firstNorm += (double)first[i] * first[i];
            secondNorm += (double)second[i] * second[i];
        }

        if (firstNorm == 0 || secondNorm == 0)
        {
            return -1;
        }

        return dotProduct / (Math.Sqrt(firstNorm) * Math.Sqrt(secondNorm));
    }
}