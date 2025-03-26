using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlogServer.Data;
using BlogLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogServer.Services
{
    public class AdminService 
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<UserRoleViewModel>> GetAllUserRolesAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Fetch roles
                userRoles.Add(new UserRoleViewModel
                {
                    UserName = user.UserName,
                    Roles = roles.ToList() // Convert to List<string>
                });
            }
            return userRoles;
        }

        // Action to update user roles (toggle Contributor role)
        public async Task<bool> UpdateRolesAsync(string userName, bool isContributor)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                if (isContributor)
                {
                    if (!await _userManager.IsInRoleAsync(user, "Contributor"))
                    {
                        await _userManager.AddToRoleAsync(user, "Contributor");
                    }
                }
                else
                {
                    if (await _userManager.IsInRoleAsync(user, "Contributor"))
                    {
                        await _userManager.RemoveFromRoleAsync(user, "Contributor");
                    }
                }
            }
            return true;
        }
    
        [ValidateAntiForgeryToken]
        public async Task<bool> BanUserAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return false;

            // First delete all articles by this user
            var articles = _context.Articles.Where(a => a.ContributorUsername == user.UserName);
            _context.Articles.RemoveRange(articles);
            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user); // Soft delete is an option also
            
            return result.Succeeded; // Return a success status
        }

        public async Task<User?> GetUserDetailsAsync(string username)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<bool> SaveUserAsync(User updatedUser)
        {
            var existingUser = await _userManager.FindByIdAsync(updatedUser.Id);
            if (existingUser == null) return false;

            _context.Entry(existingUser).CurrentValues.SetValues(updatedUser);
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        // POST: ManageUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(string username, 
        //     [Bind("FirstName,LastName,isApproved,Id,UserName,"+
        //     "NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,"+
        //     "PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,"+
        //     "PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,"+
        //     "LockoutEnabled,AccessFailedCount")] User user)
        // {
        //     if (username != user.UserName)
        //     {
        //         return NotFound();
        //     }

        //     if (ModelState.IsValid)
        //     {
        //         try
        //         {
        //             _context.Update(user);
        //             await _context.SaveChangesAsync();
        //         }
        //         catch (DbUpdateConcurrencyException)
        //         {
        //             if (!UserExists(user.UserName))
        //             {
        //                 return NotFound();
        //             }
        //             else
        //             {
        //                 throw;
        //             }
        //         }
        //         return RedirectToAction(nameof(ManageUsers));
        //     }
        //     return View(user);
        // }
        // private bool UserExists(string username)
        // {
        //     return _context.Users.Any(e => e.UserName == username);
        // }
    }
}
