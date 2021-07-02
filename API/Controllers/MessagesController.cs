using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{


    [Authorize]

    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public MessagesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {

            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot send message to yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound("We cannot find the user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageFromUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var message = await _unitOfWork.MessageRepository.GetMessageForUser(messageParams);

            Response.AddPaginationHeader(message.CurrentPage, message.PageSize, message.TotalCount, message.TotalPages);

            return message;
        }



        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteMessage(int id)
        {

            var username = User.GetUsername();

            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if (message.Sender.UserName != username && message.Recipient.UserName != username) return Unauthorized();

            if (message.Sender.UserName == username) message.SenderDeleted = true;

            if (message.Recipient.UserName == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _unitOfWork.MessageRepository.DeleteMessage(message);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete the message");
        }
    }
}