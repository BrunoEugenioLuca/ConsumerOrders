using ConsumerOrders.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;

namespace ConsumerOrders { 

public class Consumer
{
        public ReadSettings? settings;
        public MessageDto? orderDto = new MessageDto();
    
        public void PullMessage()
        {
            
            try
            {
                var factory = new ConnectionFactory()
                {
                    Uri = new Uri("amqps://nwthmpgl:4wsjpaRHSPOk-Hy7CfE1rLT289PvT5bv@turkey.rmq.cloudamqp.com/nwthmpgl")
                };

            //var connection = factory.CreateConnection();
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                //{
                //var channel = connection.CreateModel();
                channel.QueueDeclare(queue: "queueTest",
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);


                settings = new ReadSettings();
                settings.AppSettingsJson();
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var data = Encoding.ASCII.GetString(body);
                    orderDto = JsonSerializer.Deserialize<MessageDto>(data);

                    Console.WriteLine($"{orderDto.OrderId}" +
                        $"{orderDto.OrderDate}" +
                        $"{orderDto.Total}"+
                        $"{orderDto.FirstName}" +
                        $"{orderDto.LastName}" +
                        $"{orderDto.FiscalCode}" +
                        $"{orderDto.Email}"+
                        $"{orderDto.Phone}"+
                        $"{orderDto.PIva}"+
                        $"{orderDto.Address}" +
                        $"{orderDto.City}"+
                        $"{orderDto.Cap}"
                        
                    );

                    foreach(var item in orderDto.ProductsList)
                    {
                        Console.WriteLine($"{item.Id}" +
                            $"{item.ProductName}" +
                            $"{item.Price}" +
                            $"{item.ShortDescription}" +
                            $"{item.FullDescription}" +
                            $"{item.Published}");
                    }

                    Task.Run(async () =>
                    {
                        await RequestApiAsync();
                    });
                };
                channel.BasicConsume(queue: "queueTest",
                                        autoAck: true,
                                        consumer: consumer);

                
                Console.ReadLine();


            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error: {ex.Message}");
            }
           
        }

        public async Task RequestApiAsync()
        {
            try
            {

                var url = "https://api.fattureincloud.it/v1/ordini/nuovo";
                var clientInsert = new RestClient(url);
                var requestInsert = new RestRequest();

                var totaleNetto = 0M;
                List<Lista_Articoli> articoli = new List<Lista_Articoli>();
                
                for(int i = 0; i < orderDto.ProductsList.Count; i++)
                {
                    var item = orderDto.ProductsList.ElementAt(i);
                    articoli.Add(new Lista_Articoli
                    {
                        id = Convert.ToString(item.Id),
                        nome = item.ProductName,
                        quantita = 1,
                        descrizione = item.FullDescription,
                        prezzo_netto = (int) item.Price,
                        prezzo_lordo = 0,
                        cod_iva = 0,
                        ordine = i,
                        tassabile = true,
                        sconto = 0,
                        applica_ra_contributi = true,
                        sconto_rosso = 0,
                        magazzino = true
                    }) ;
                    totaleNetto += (int)item.Price;
                }
                var iva = (decimal) (totaleNetto * 22) / 100;
                //var importo = iva + totaleNetto;
                var pagamento = new List<Lista_Pagamenti>();
                pagamento.Add(new Lista_Pagamenti
                { 
                    data_scadenza = orderDto.OrderDate.ToString("dd/MM/yyyy"),
                    importo = Convert.ToString(iva+totaleNetto),
                    metodo = "not",
                    data_saldo = orderDto.OrderDate.ToString("dd/MM/yyyy")
                });
                var anagrafica = new Extra_Anagrafica
                {
                    mail = orderDto.Email,
                    tel = orderDto.Phone
                };
                var body = new OrdineDto
                {
                    api_uid = settings.Uid,
                    api_key = settings.Key,
                    nome = orderDto.FirstName+" "+orderDto.LastName,
                    indirizzo_via = orderDto.Address,
                    indirizzo_cap = orderDto.Cap,
                    indirizzo_citta = orderDto.City,
                    piva = orderDto.PIva,
                    cf = orderDto.FiscalCode,
                    autocompila_anagrafica = false,
                    salva_anagrafica = false,
                    valuta = "EUR",
                    
                    valuta_cambio = 1,
                    centro_costo = "",
                    lista_pagamenti = pagamento.ToArray(),
                    lista_articoli = articoli.ToArray(),
                    extra_anagrafica = anagrafica,
                    imponibile_ritenuta = 0,
                    mostra_totali = "tutti",
                    metodo_pagamento = "Bonifico",
                   

                };

                requestInsert.AddJsonBody(body);
                var response = await clientInsert.PostAsync(requestInsert);
                Console.WriteLine($"{response.StatusCode} {response.Content}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
            }
        }

        static void Main(string[] args)
        {
            new Consumer().PullMessage();
        }
    }
}
