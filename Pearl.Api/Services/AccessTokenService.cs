﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pearl.Api.Options;

namespace Pearl.Api.Services;

public sealed class AccessTokenService
{
    private readonly IOptions<SecretsOptions> secretsOptions;
    private readonly TokenValidationParameters tokenValidationParameters;

    public AccessTokenService(IOptions<SecretsOptions> secretsOptions,
        TokenValidationParameters tokenValidationParameters)
    {
        this.secretsOptions = secretsOptions;
        this.tokenValidationParameters = tokenValidationParameters;
    }

    public string Generate(string userName)
    {
        var claims = new Claim[]
        {
            // The reason behind using the subject claim is because all user names are unique.
            new Claim("sub", userName)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretsOptions.Value.Key));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(secretsOptions.Value.AccessTokenLifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Result<string?> Verify(string token)
    {
        try
        {
            tokenValidationParameters.ValidateLifetime = false;

            var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out _);

            return Result.Ok(claimsPrincipal.Claims
                .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value);
        }
        catch
        {
            return Result.Fail("The provided access token is invalid.");
        }
    }
}