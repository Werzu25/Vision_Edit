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
    
    public async Task<UserModel> LoginUser(LoginModel loginModel)
    {
        UserModel user = await _dbManager.Users.FirstOrDefaultAsync(u => u.Username == loginModel.Username);
        if (user == null) 
            return null;
        PasswordHasher<UserModel> passwordHasher = new PasswordHasher<UserModel>();
        if(passwordHasher.VerifyHashedPassword(user,user.Password,loginModel.Password) == PasswordVerificationResult.Success)
            return user;
        return null;
    }

    public async Task<UserModel> CreateUser(UserModel userModel)
    {
        PasswordHasher<UserModel> passwordHasher = new PasswordHasher<UserModel>();
        userModel.Password = passwordHasher.HashPassword(userModel, userModel.Password);
        var createdUser = (await _dbManager.AddAsync(userModel)).Entity;
        if (await _dbManager.SaveChangesAsync() > 0)
        {
            return createdUser;
        }

        return null;
    }

    public async Task<UserModel?> GetUserByUsername(string username)
    {
        return await _dbManager.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
}