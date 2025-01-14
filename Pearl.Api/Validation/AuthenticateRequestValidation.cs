﻿using FluentValidation;
using Pearl.Models;

namespace Pearl.Api.Validation;

public sealed class AuthenticateRequestValidation : AbstractValidator<AuthenticateRequest>
{
    public AuthenticateRequestValidation()
    {
        RuleFor(request => request.UserName).NotEmpty();
        RuleFor(request => request.Password).NotEmpty();
    }
}