using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM.Services;

public class UserService
{
    private readonly DbManager _dbManager;
    
    public UserService(DbManager dbManager)
    {
        _dbManager = dbManager;
    }
    
    public async Task<List<UserModel>> GetAllUsers()
    {
        return await _dbManager.Users.ToListAsync();
    }

    public async Task<UserModel> GetUserById(int userId)
    {
        return await _dbManager.Users.FindAsync(userId);
    }

    public async Task<UserModel> CreateUser(UserModel userModel)
    {
        PasswordHasher<UserModel> passwordHasher = new PasswordHasher<UserModel>();
        passwordHasher.HashPassword(userModel, userModel.Password);
        return (await _dbManager.AddAsync(userModel)).Entity;
    }
}