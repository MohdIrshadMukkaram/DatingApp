using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly IUserRepository __userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository _userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            __userRepository = _userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery]UserParams userparams)
        {
            var user = await __userRepository.GetUserByUsernameAsync(User.GetUsername());
            userparams.CurrentUsername = User.GetUsername();

            if(string.IsNullOrEmpty(userparams.Gender))
                userparams.Gender = user.Gender == "male" ? "female" : "male";
            var users = await __userRepository.GetMembersAsync(userparams);

            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }

        //  api/users/lina

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            return await __userRepository.GetMemberAsync(username);

        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await __userRepository.GetUserByUsernameAsync(User.GetUsername());
            _mapper.Map(memberUpdateDto, user);
            __userRepository.Update(user);

            if (await __userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]

        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            //Getting user eagerly loading
            var user = await __userRepository.GetUserByUsernameAsync(User.GetUsername());
            //result from photo service 
            var result = await _photoService.AddPhotoAsync(file);
            //check result
            if(result.Error != null) return BadRequest(result.Error.Message);
            //new photo
            var photo = new Photo {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            //if any photo in the user collection if it isn't than the photo will be main
            if(user.Photos.Count == 0) {
                photo.IsMain = true;
            }

            //Add Photo 
            user.Photos.Add(photo);

            //Save it in the DATABASE.
            if(await __userRepository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new {username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId) {
            var user = await __userRepository.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if(photo.IsMain) return BadRequest("this is already your main photo");
            
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if(await __userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set Profile Photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId) {
            var user = await __userRepository.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("You cannot delete you profile photo");

            if(photo.PublicId != null) {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);

            if(await __userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete Photo");
        }
    }
}