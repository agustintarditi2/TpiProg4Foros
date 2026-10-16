

using MyApp.Application.Abstractions;
using MyApp.Application.Interfaces;
using MyApp.Application.Models;

namespace MyApp.Application.Services;

public class AuthServices
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    public AuthServices(
        IUserRepository userRepository, 
        IPasswordHasher hasher
        )
    {
        _repo = userRepository;
        _hasher = hasher;
    }
    
    public async Task<string> Authenticate(LoginUserDTO loginData)
    {
        
        return "";
    }

}