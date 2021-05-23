using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interface
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId,int likeUserId);

        Task<AppUser> GetUserWithLikes(int userId);

        Task<PageList<LikeDto>> GetUserLikes(LikedParams likedParams);
    }
}