using BetaClientesVM.Funciones;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PMLLogica.ConexionesApi
{
    public  class ComunicacionServiciosApi
    {
        /// <summary>
        /// Variables globales para la comunicación con la API Beta.
        /// </summary>
        private readonly int idUsuario;
        private readonly int idPlanta;
        private readonly int idEmpresa;
        private readonly string urlApiBeta;


        /// <summary>
        /// Constructor de la clase ComunicacionServiciosApi.
        /// </summary>
        /// <param name="idUsuario">El ID del usuario para la comunicación con la API.</param>
        /// <param name="idPlanta">El ID de la planta para la comunicación con la API.</param>
        /// <param name="idEmpresa">El ID de la empresa para la comunicación con la API.</param>
        /// <param name="urlApiBeta">La URL de la API para la comunicación.</param>
        public ComunicacionServiciosApi(int idUsuario, int idPlanta, int idEmpresa, string urlApiBeta)
        {
            this.idUsuario = idUsuario;
            this.idPlanta = idPlanta;
            this.idEmpresa = idEmpresa;
            this.urlApiBeta = urlApiBeta;
        }



        /// <summary>
        /// JGPJ 15/04/2024 Método para escribir en la bitácora de errores de la aplicación.
        /// Este método envía información sobre un error a un servicio web para su registro en la bitácora.
        /// </summary>
        /// <param name="reporteError">Descripción del error ocurrido.</param>
        /// <param name="source">Ruta o ubicación donde se generó el error.</param>
        /// <param name="userLogin">ID del usuario que inició sesión cuando ocurrió el error.</param>
        /// <param name="type">Tipo de error (p. ej., error de lógica de negocio, error de sistema).</param>
        /// <param name="receiver">Nombre del proyecto o componente donde se generó el error.</param>
        /// <returns>Devuelve true si el registro en la bitácora se realizó con éxito; de lo contrario, devuelve false.</returns>
        public async Task<bool> EscribirEnBitacora(string reporteError, string source, string userLogin, string type, string receiver)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {

                    Bitacora_ErroresVM objBitacora = new Bitacora_ErroresVM
                    {
                        REPORTE_ERROR = reporteError,
                        SOURCE = source,
                        USER_LOGIN = userLogin,
                        TYPE = type,
                        RECEIVER = receiver
                    };

                    var jsonData = JsonConvert.SerializeObject(objBitacora);
                    var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    string url                           =   $"{urlApiBeta}/{"ApiBitacoraErrores/BitacoraErroresCliente"}";
                    HttpResponseMessage resultResponse   =   await httpClient.PostAsync(url, httpContent);

                    if (resultResponse.IsSuccessStatusCode)
                    {
                        string content = await resultResponse.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<bool>(content);
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// JGPJ 15/04/2024 Método que establece comunicación con la api mediante una petición GET
        /// </summary>
        /// <param name="url">Url de comunicación con la api (Controller/Método)</param>
        /// <returns>Nos retorna la información como un DataTable</returns>
        public async Task<DataTable> DTObtenerDatosApi(string url)
        {
            try
            {
                DataTable dt = new DataTable();
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(urlApiBeta + url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dt = JsonConvert.DeserializeObject<DataTable>(responseBody);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                await EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/DTObtenerDatosApi", "0", "Error", "Portal_BetaClientes");
                return null;
            }

        }


        /// <summary>
        /// Método asincrónico para obtener datos de una API.
        /// JGPJ 15/04/2024 
        /// </summary>
        /// <param name="stUrl">URL a conectar de la API</param>
        /// <param name="jsonData">Objeto de datos pasado por JSON</param>
        /// <returns>Un objeto DataTable que contiene los datos obtenidos si la solicitud es exitosa; de lo contrario, devuelve null.</returns>
        public async Task<DataTable> DTObtenerDatosApi(string stUrl, string jsonData)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    string url = $"{urlApiBeta}/{stUrl}";
                    HttpResponseMessage resultResponse = await httpClient.PostAsync(url, httpContent);
                    if (resultResponse.IsSuccessStatusCode)
                    {
                        string content = await resultResponse.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<DataTable>(content);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                await EscribirEnBitacora(ex.Message, "PMLLogica/ComunicacionServiciosApi/DTObtenerDatosApi()", idUsuario.ToString(), "Error", "PMLLogica");
                return null;
            }

        }


        /// <summary>
        /// Método para gestionar las operaciones CRUD en la API (Insertar, Actualizar, Eliminar).
        /// Este método envía una solicitud POST a la URL especificada con los datos JSON proporcionados.
        /// Si la solicitud es exitosa, devuelve el resultado como un entero.
        /// Si no es exitosa, devuelve 0.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <param name="stUrl">La parte específica de la URL para la solicitud.</param>
        /// <param name="jsonData">Los datos en formato JSON para enviar en la solicitud.</param>
        /// <returns>Un entero que representa el resultado de la operación. Devuelve 0 si hay algún error.</returns>
        public async Task<int> GestionarCRUD(string stUrl, string jsonData)
        {
            try
            {

                using (var httpClient = new HttpClient())
                {
                    var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    string url = $"{urlApiBeta}/{stUrl}";
                    HttpResponseMessage resultResponse = await httpClient.PostAsync(url, httpContent);
                    if (resultResponse.IsSuccessStatusCode)
                    {
                        string content = await resultResponse.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<int>(content);
                    }
                    else if(resultResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        string errorMessage = await resultResponse.Content.ReadAsStringAsync();
                        await EscribirEnBitacora(errorMessage, "PMLLogica/ComunicacionServiciosApi/GestionarCRUD()", idUsuario.ToString(), "Error", "PMLLogica");
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await EscribirEnBitacora(ex.Message, "PMLLogica/ComunicacionServiciosApi/GestionarCRUD()", idUsuario.ToString(), "Error", "PMLLogica");
                return 0;
            }

        }






    }
}
