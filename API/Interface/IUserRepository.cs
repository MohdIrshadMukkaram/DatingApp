using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interface
{
    public interface IUserRepository
    {
        void Update(AppUser user);


        Task<IEnumerable<AppUser>> GetUserAsync();

        Task<AppUser> GetUserByIdAsync(int id);

        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<PageList<MemberDTO>> GetMembersAsync(UserParams userParams);

        Task<MemberDTO> GetMemberAsync(string username);

        Task<string> GetUserGender(string username);
        
    }
}