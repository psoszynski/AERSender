using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AERSender
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine(" [x] Received {0}", message);

                    var o = ObjectToXML<AERMessage>(message);
                };
                channel.BasicConsume(queue: "hello", autoAck: false, consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        public static T FromXml<T>(string message)
        {
            T o;
            using (MemoryStream stream = new MemoryStream())
            {
                SoapReflectionImporter soap = new SoapReflectionImporter();
                var mapping = soap.ImportTypeMapping(typeof(T));
                XmlSerializer s = new XmlSerializer(mapping);

                stream.Position = 0;
                var xmlReader = new XmlTextReader(stream);
                o = (T)s.Deserialize(xmlReader, message);
                xmlReader.Close();
            }
            return o;
        }

        public static T ObjectToXML<T>(string xml)
        {
            StringReader strReader = null;
            XmlSerializer serializer = null;
            XmlTextReader xmlReader = null;
            T obj = default(T);
            try
            {
                strReader = new StringReader(xml);
                serializer = new XmlSerializer(typeof(T));
                xmlReader = new XmlTextReader(strReader);
                obj = (T)serializer.Deserialize(xmlReader);
            }
            catch (Exception exp)
            {
                //Handle Exception Code
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
                if (strReader != null)
                {
                    strReader.Close();
                }
            }
            return obj;
        }

    }

    [XmlRoot(ElementName = "AERMessage", Namespace = "http://tempuri.org/")]
    public class AERMessage
    {
        [XmlElement(ElementName = "Content", Namespace = "http://tempuri.org/")]
        public string Content { get; set; }

        [XmlElement(ElementName = "Id", Namespace = "http://tempuri.org/")]
        public string Id { get; set; }
    }

    public class R3
    {
        public string Type { get; set; }

        public DateTime StartDate { get; set; }

        public string Name { get; set; }
    }
}
