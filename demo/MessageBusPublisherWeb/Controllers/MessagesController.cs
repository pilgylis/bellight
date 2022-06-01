
using Bellight.MessageBus.Abstractions;
using MessageBusPublisherWeb.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MessageBusPublisherWeb.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageBusFactory messageBusFactory;
        private readonly IConfiguration configuration;

        public MessagesController(IMessageBusFactory messageBusFactory, IConfiguration configuration)
        {
            this.messageBusFactory = messageBusFactory;
            this.configuration = configuration;
        }

        // POST api/<MessagesController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Message message)
        {
            var topic = configuration["ServiceBus:Topic"];
            var messageBusType = "PubSub".Equals(configuration["ServiceBus:Type"], StringComparison.InvariantCultureIgnoreCase) ?
                MessageBusType.PubSub : MessageBusType.Queue;
            var publisher = messageBusFactory.GetPublisher(topic, messageBusType);

            await publisher.SendAsync(message.Content);
            return Ok();
        }
    }
}
