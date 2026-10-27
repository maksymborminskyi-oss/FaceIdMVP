using System.ComponentModel.DataAnnotations;
using FaceId.Api.DTOs;

namespace FaceId.Api.Validators;

public class UpdateUserRequestValidator
{
    public bool IsValid(UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return false;
        }

        var emailValidator = new EmailAddressAttribute();

        if (!emailValidator.IsValid(request.Email))
        {
            return false;
        }

        return true;
    }
}