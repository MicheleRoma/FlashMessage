﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Security;


namespace Vereyon.Web
{
    public class FlashMessage
    {

        /// <summary>
        /// Gets / sets the transport used for the flash messages.
        /// </summary>
        public static IFlashMessageTransport Transport { get; set; }

        static FlashMessage()
        {

            Transport = new FlashMessageCookieTransport();
        }

        /// <summary>
        /// Queues a flash message for display with the specified message for display as a info notification.
        /// </summary>
        /// <param name="message"></param>
        public static void Queue(string message)
        {
            Queue(message, string.Empty, FlashMessageType.Info, false);
        }

        /// <summary>
        /// Queues a flash message for display with the specified message.
        /// </summary>
        /// <param name="message"></param>
        public static void Queue(string message, FlashMessageType messageType)
        {
            Queue(message, string.Empty, messageType, false);
        }

        /// <summary>
        /// Queues a flash message for display with the specified message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="messageType"></param>
        /// <param name="isHtml"></param>
        public static void Queue(string message, string title, FlashMessageType messageType, bool isHtml)
        {

            // Append the new message.
            var flashMessage = new FlashMessageModel { IsHtml = isHtml, Message = message, Title = title, Type = messageType };
            Queue(flashMessage);
        }

        /// <summary>
        /// Queues the passed flash message for display.
        /// </summary>
        /// <param name="message"></param>
        public static void Queue(FlashMessageModel message)
        {

            List<FlashMessageModel> messages;

            // Retrieve the currently queued cookies.
            messages = GetQueued();

            // Append the new message.
            messages.Add(message);

            // Store the messages.
            Transport.Queue(messages);
        }

        /// <summary>
        /// Retrieves the queued messages for the current response.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<FlashMessageModel> GetQueued()
        {
            return Transport.GetQueued();
        }

        /// <summary>
        /// Retrieves the queued flash messages for display and clears them.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<FlashMessageModel> Retrieve(HttpContextBase context)
        {
            return Transport.Retrieve();
        }

        /// <summary>
        /// Deserializes a serialized collection of flash messages.
        /// </summary>
        /// <param name="serializedMessages"></param>
        /// <returns></returns>
        public static List<FlashMessageModel> Deserialize(byte[] data)
        {

            var messages = new List<FlashMessageModel>();
            int messageCount;

            // Check if there is any data to read, if not we are done quickly.
            if (data.Length == 0)
                return messages;

            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {

                    // Read the number of message in the stream and deserialize each message.
                    messageCount = reader.ReadInt32();
                    while (messageCount > 0)
                    {

                        var model = new FlashMessageModel();
                        model.IsHtml = reader.ReadBoolean();
                        model.Message = reader.ReadString();
                        model.Title = reader.ReadString();
                        model.Type = (FlashMessageType)reader.ReadByte();

                        // Store message and decrement message counter.
                        messages.Add(model);
                        messageCount--;
                    }

                    return messages;
                }
            }
        }

        /// <summary>
        /// Serializes the passed list of messages to binary format.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static byte[] Serialize(IList<FlashMessageModel> messages)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {

                    // Write the number of message and serialize each message.
                    writer.Write(messages.Count);
                    foreach (var message in messages)
                    {
                        writer.Write(message.IsHtml);
                        writer.Write(message.Message);
                        writer.Write(message.Title);
                        writer.Write((byte)message.Type);
                    }

                    // Return the data as a byte array.
                    writer.Flush();
                    return stream.ToArray();
                }
            }
        }
    }
}
