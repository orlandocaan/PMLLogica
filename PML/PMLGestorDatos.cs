using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using BetaClientesVM.Funciones;
using BetaClientesVM.PML;
using Newtonsoft.Json;
using PMLLogica.ConexionesApi;
namespace PMLLogica.PML
{
    public class PMLGestorDatos
    {
        /// <summary>
        /// Definiendo variables de clase.
        /// </summary>
        private readonly int idUsuario;
        private readonly int idPlanta;
        private readonly int idEmpresa;
        private readonly string urlApi;
        private readonly string idErp;
        private readonly ComunicacionServiciosApi objComServApi;



        /// <summary>
        /// Constructor de la clase DatosApiClientes.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <param name="idUsuario">ID del usuario asociado a los datos del cliente.</param>
        /// <param name="idPlanta">ID de la planta asociada a los datos del cliente.</param>
        /// <param name="idEmpresa">ID de la empresa asociada a los datos del cliente.</param>
        /// <param name="urlApi">URL de la API utilizada para acceder a los datos del cliente.</param>
        /// <param name="erp">Identificador del sistema ERP utilizado.</param>
        public PMLGestorDatos(int idUsuario, int idPlanta, int idEmpresa, string urlApi, string idErp)
        {

            this.idUsuario = idUsuario;
            this.idPlanta = idPlanta;
            this.idEmpresa = idEmpresa;
            this.urlApi = urlApi;
            this.idErp = idErp;

            // Instanciamos la clase ComunicacionServiciosApi
            objComServApi = new ComunicacionServiciosApi(idUsuario, idPlanta, idEmpresa, urlApi);
        }


        /// <summary>
        /// Método que nos permitirá obtener los tipos de equipos por planta
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Nos retorna una lista de ComboVM</returns>
        public async Task<List<ComboVM>> ComboTipoEquipos()
        {
            try
            {
                List<ComboVM> listaTipoEquipo = new List<ComboVM>();

                CatalogosVM objCatalogo = new CatalogosVM
                {
                    Accion = 1,
                    Catt_Nombre = "PMLTipoEquipos"
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiDatos/DTConsultaCatalogos", JsonConvert.SerializeObject(objCatalogo));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaTipoEquipo = dtConsulta.AsEnumerable()
                                                .Select(dataRow => new ComboVM
                                                {
                                                    TextoOpcion = dataRow.Field<string>("Catv_Nombre") ?? "",
                                                    ValorEntero = Convert.ToInt32(dataRow.Field<object>("Catv_IdOpcion")),
                                                }).ToList();
                }

                return listaTipoEquipo;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ComboTipoEquipos", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Método que nos permitirá obtener las Áreas asociadas al id de la planta
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Nos retorna una lista de ComboVM</returns>
        public async Task<List<ComboVM>> ComboAreas()
        {
            try
            {
                List<ComboVM> listaAreas = new List<ComboVM>();

                PMLAreasVM objAreas = new PMLAreasVM
                {
                    Accion = 1,
                    Area_IdPlanta = idPlanta
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaAreas", JsonConvert.SerializeObject(objAreas));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaAreas = dtConsulta.AsEnumerable()
                                           .Select(dataRow => new ComboVM
                                           {
                                               TextoOpcion = dataRow.Field<string>("Area_Nombre") ?? "",
                                               ValorEntero = Convert.ToInt32(dataRow.Field<object>("Area_IdArea")),
                                           }).ToList();
                }
                return listaAreas;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ComboAreas", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Este Método nos permite consultar los equipos asociados a una planta
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Nos retorna una Lista de PMLEquiposVM</returns>
        public async Task<List<PMLEquiposVM>> ListaEquiposPlanta()
        {
            try
            {
                List<PMLEquiposVM> listaEquipos = new List<PMLEquiposVM>();

                PMLEquiposVM objEquipos = new PMLEquiposVM
                {
                    Accion = 1,
                    IdUsuario = idUsuario,
                    Equi_IdPlanta = idPlanta
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaEquipos", JsonConvert.SerializeObject(objEquipos));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaEquipos = dtConsulta.AsEnumerable()
                                             .Select(dataRow => new PMLEquiposVM
                                             {
                                                 Equi_IdEquipo = Convert.ToInt32(dataRow.Field<object>("Equi_IdEquipo") ?? 0),
                                                 Equi_Nombre = dataRow.Field<string>("Equi_Nombre") ?? "",
                                                 Equi_TipoEquipo = Convert.ToInt32(dataRow.Field<object>("Equi_TipoEquipo") ?? 0),
                                                 NombreTipoEquipo = dataRow.Field<string>("NombreTipoEquipo") ?? "",
                                                 Equi_IdArea = Convert.ToInt32(dataRow.Field<object>("Equi_IdArea") ?? 0),
                                                 NombreArea = dataRow.Field<string>("NombreArea"),
                                                 Equi_POES = dataRow.Field<string>("Equi_POES") ?? "",
                                                 Equi_IdTurno = Convert.ToInt32(dataRow.Field<object>("Equi_IdTurno") ?? 0),
                                                 NombreTurno = dataRow.Field<string>("NombreTurno") ?? ""
                                             }).ToList();
                }

                return listaEquipos;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ListaEquiposPlanta()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Método que nos permitirá obtener los Turnos 
        /// GFLT 22/04/2024 
        /// </summary>
        /// <returns>Nos retora una Lista de ComboVM</returns>
        public async Task<List<ComboVM>> ComboTurnos()
        {
            try
            {
                List<ComboVM> listaTurnos = new List<ComboVM>();

                PMLTurnosVM objTurnos = new PMLTurnosVM
                {
                    Accion = 0,
                    Tur_IdPlanta = idPlanta,
                    IdUsuario = idUsuario,
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaTurnos", JsonConvert.SerializeObject(objTurnos));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaTurnos = dtConsulta.AsEnumerable()
                                            .Select(dataRow => new ComboVM
                                            {
                                                TextoOpcion = dataRow.Field<string>("Tur_Descripcion") ?? "",
                                                ValorEntero = Convert.ToInt32(dataRow.Field<object>("Tur_Id")),
                                            }).ToList();
                }
                return listaTurnos;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ComboTurnos", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
        /// JGPPJ 15/04/2024
        /// </summary>
        /// <param name="objEquipos">Objeto de datos de PMLEquiposVM</param>
        /// <returns>
        /// Un objeto Task que representa la operación asincrónica. 
        /// La tarea devolverá un objeto que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> GestionarEquipos(PMLEquiposVM objEquipos)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;


                objEquipos.IdUsuario = idUsuario;
                objEquipos.Equi_UsuarioCrea = idUsuario;
                objEquipos.Equi_UsuarioMod = idUsuario;
                objEquipos.Equi_IdPlanta = idPlanta;

                resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarEquipos", JsonConvert.SerializeObject(objEquipos));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ha ocurrido un error durante la operación.";
                }
                else
                {
                    estatus = true;
                    mensaje = "La operación se ha completado correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/GestionarEquipos", idUsuario.ToString(), "Error", "Portal");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }


        /// <summary>
        /// Este Método nos permite consultar los colaboradores
        /// OCA 16/04/2024
        /// </summary>
        /// <returns>Nos retorna una Lista de PMLColaboradoresVM</returns>
        public async Task<List<PMLColaboradoresVM>> ListaColaboradores()
        {
            try
            {
                List<PMLColaboradoresVM> listaColaboradores = new List<PMLColaboradoresVM>();

                PMLEquiposVM objEquipos = new PMLEquiposVM
                {
                    Accion = 0,
                    IdUsuario = idUsuario,
                    Equi_IdPlanta = idPlanta
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaColaboradores", JsonConvert.SerializeObject(objEquipos));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaColaboradores = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new PMLColaboradoresVM
                                   {
                                       Col_IdColaborador = Convert.ToInt32(dataRow.Field<object>("Col_IdColaborador") ?? 0),
                                       Col_Nombre = dataRow.Field<string>("Col_Nombre"),
                                       Col_Puesto = Convert.ToInt32(dataRow.Field<object>("Col_Puesto") ?? 0),
                                       Col_Activo = Convert.ToBoolean(dataRow.Field<object>("Col_Activo") ?? false),
                                       Col_UsuarioCrea = Convert.ToInt32(dataRow.Field<object>("Col_UsuarioCrea") ?? 0),
                                       Col_UsuarioMod = Convert.ToInt32(dataRow.Field<object>("Col_UsuarioMod") ?? 0),
                                       Col_FechaCrea = Convert.ToDateTime(dataRow.Field<object>("Col_FechaCrea") ?? DateTime.MinValue),
                                       Col_FechaMod = Convert.ToDateTime(dataRow.Field<object>("Col_FechaMod") ?? DateTime.MinValue),
                                       Pue_Nombre = dataRow.Field<string>("NombrePuesto"),
                                       Col_IdPlanta = Convert.ToInt32(dataRow.Field<object>("Col_IdPlanta") ?? 0)
                                   }).ToList();
                }

                return listaColaboradores;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ListaColaboradores()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
        /// OCA 16/04/2024
        /// </summary>
        /// <param name="objColaborador">Objeto de datos de PMLColaboradoresVM</param>
        /// <returns>
        /// Un objeto Task que representa la operación asincrónica. 
        /// La tarea devolverá un objeto anónimo que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> GestionarColaboradores(PMLColaboradoresVM objColaborador)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;


                objColaborador.IdUsuario = idUsuario;
                objColaborador.Col_UsuarioCrea = idUsuario;
                objColaborador.Col_UsuarioMod = idUsuario;
                objColaborador.Col_IdPlanta = idPlanta;

                resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarColaboradores", JsonConvert.SerializeObject(objColaborador));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/GestionarEquipos", idUsuario.ToString(), "Error", "Portal");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }


        /// <summary>
        /// Este Método nos permite consultar los Puestos
        /// OCA 17/04/2024
        /// </summary>
        /// <returns>Nos retorna una Lista de ComboVM</returns>
        public async Task<List<ComboVM>> ListaPuestos()
        {
            try
            {
                List<ComboVM> listaColaboradores = new List<ComboVM>();

                CatalogosVM objPuestos = new CatalogosVM
                {
                    Accion = 0,
                    Catt_Nombre = "PMLPuestos",
                    IdUsuario = idUsuario,
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaPuestos", JsonConvert.SerializeObject(objPuestos));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaColaboradores = dtConsulta.AsEnumerable()
                                      .Select(dataRow => new ComboVM
                                      {
                                          TextoOpcion = dataRow.Field<string>("Catv_Nombre") ?? "",
                                          ValorEntero = Convert.ToInt32(dataRow.Field<object>("Catv_IdOpcion")),
                                      }).ToList();
                }

                return listaColaboradores;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ListaPuestos()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }

        /// <summary>
        /// Método que nos permitirá gestionar los Turnos del CRUD.
        /// GFLT 22/04/2024
        /// </summary>
        /// <param name="objCursos">Objeto de datos de PMLCursosVM</param>
        /// <returns>
        /// Un objeto Task que representa la operación asincrónica. 
        /// La tarea devolverá un objeto anónimo que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns
        public async Task<object> GestionarCursos(PMLCursosVM objCursos)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;


                objCursos.IdUsuario = idUsuario;
                objCursos.Cur_UsuarioCrea = idUsuario;
                objCursos.Cur_UsuarioMod = idUsuario;
                objCursos.Cur_IdPlanta = idPlanta;

                resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarCursos", JsonConvert.SerializeObject(objCursos));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/GestionarEquipos", idUsuario.ToString(), "Error", "Portal");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }


        /// <summary>
        /// Este método nos permite consultar los datos de un equipo en específico.
        /// JGPJ 16/04/2024
        /// </summary>
        /// <param name="idEquipo">Identificador del equipo a consultar.</param>
        /// <returns>Nos devuelve un objeto de PMLEquiposVM</returns>
        public async Task<PMLEquiposVM> ConsultarDatosEquipo(int idEquipo)
        {
            try
            {
                PMLEquiposVM objEquipo = new PMLEquiposVM
                {
                    Accion = 5,
                    IdUsuario = idUsuario,
                    Equi_IdEquipo = idEquipo,
                    Equi_IdPlanta = idPlanta
                };

                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaEquipos", JsonConvert.SerializeObject(objEquipo));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    objEquipo = dtConsulta.AsEnumerable()
                                           .Select(dataRow => new PMLEquiposVM
                                           {
                                               Equi_IdEquipo = Convert.ToInt32(dataRow.Field<object>("Equi_IdEquipo") ?? 0),
                                               Equi_Nombre = dataRow.Field<string>("Equi_Nombre") ?? "",
                                               Equi_TipoEquipo = Convert.ToInt32(dataRow.Field<object>("Equi_TipoEquipo") ?? 0),
                                               Equi_IdArea = Convert.ToInt32(dataRow.Field<object>("Equi_IdArea") ?? 0),
                                               Equi_POES = dataRow.Field<string>("Equi_POES") ?? "",
                                               Equi_IdTurno = Convert.ToInt32(dataRow.Field<object>("Equi_IdTurno") ?? 0),
                                               Equi_IdPlanta = Convert.ToInt32(dataRow.Field<object>("Equi_IdPlanta") ?? 0),
                                               NombreTipoEquipo = dataRow.Field<string>("NombreTipoEquipo") ?? "",
                                               NombreArea = dataRow.Field<string>("NombreArea") ?? "",
                                               NombreTurno = dataRow.Field<string>("NombreTurno") ?? ""

                                           }).FirstOrDefault();
                }
                else
                {
                    objEquipo = new PMLEquiposVM();
                }

                return objEquipo;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ConsultarDatosEquipo", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Método que nos permitirá obtener los tipos de productos del catalogo de opciones
        /// JGPJ 17/04/2024
        /// </summary>
        /// <returns>Nos retorna una lista de ComboVM</returns>
        public async Task<List<ComboVM>> ComboTipoProductos()
        {
            try
            {
                List<ComboVM> listaCombo = new List<ComboVM>();

                CatalogosVM objCatalogo = new CatalogosVM
                {
                    Accion = 1,
                    Catt_Nombre = "PMLTipoProducto"
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiDatos/DTConsultaCatalogos", JsonConvert.SerializeObject(objCatalogo));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaCombo = dtConsulta.AsEnumerable()
                                                .Select(dataRow => new ComboVM
                                                {
                                                    TextoOpcion = dataRow.Field<string>("Catv_Nombre") ?? "",
                                                    ValorEntero = Convert.ToInt32(dataRow.Field<object>("Catv_IdOpcion")),
                                                }).ToList();
                }

                return listaCombo;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ComboTipoProductos", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Nos retorna una lista de las unidades de medida
        /// JGPJ 17/04/2024
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComboVM>> ComboUnidadesDeMedida()
        {
            try
            {
                List<ComboVM> listaCombo = new List<ComboVM>();

                CatalogosVM objCatalogo = new CatalogosVM
                {
                    Accion = 1,
                    Catt_Nombre = "PMLUnidadesDeMedida"
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiDatos/DTConsultaCatalogos", JsonConvert.SerializeObject(objCatalogo));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaCombo = dtConsulta.AsEnumerable()
                                                .Select(dataRow => new ComboVM
                                                {
                                                    TextoOpcion = dataRow.Field<string>("Catv_Nombre") ?? "",
                                                    ValorEntero = Convert.ToInt32(dataRow.Field<object>("Catv_IdOpcion")),
                                                }).ToList();
                }

                return listaCombo;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ComboTipoProductos", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }



        /// <summary>
        /// Este método nos permitirá consultar los tipos de limpieza del catalogo de tipo  y de opciones
        /// JGPJ 17/04/2024
        /// </summary>
        /// <returns>Nos retorna una Lista de ComboVM</returns>
        public async Task<List<ComboVM>> ComboTiposDeLimpieza()
        {
            try
            {
                List<ComboVM> listaCombo = new List<ComboVM>();

                CatalogosVM objCatalogo = new CatalogosVM
                {
                    Accion = 1,
                    Catt_Nombre = "PMLTipoLimpieza"
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiDatos/DTConsultaCatalogos", JsonConvert.SerializeObject(objCatalogo));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaCombo = dtConsulta.AsEnumerable()
                                                .Select(dataRow => new ComboVM
                                                {
                                                    TextoOpcion = dataRow.Field<string>("Catv_Nombre") ?? "",
                                                    ValorEntero = Convert.ToInt32(dataRow.Field<object>("Catv_IdOpcion")),
                                                }).ToList();
                }

                return listaCombo;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ComboTiposDeLimpieza", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Este método nos permitirá consultar la lista de frecuencias asociadas a un equipo
        /// JGOJ 23/04/2024
        /// </summary>
        /// <param name="idEquipo">Id del equipo a consultar</param>
        /// <returns>Nos retorna una lista de PMLFrecuenciasVM</returns>
        public async Task<List<PMLFrecuenciaVM>> ListaFrecuenciaEquipo(int idEquipo)
        {
            try
            {
                List<PMLFrecuenciaVM> listaFrecuencia = new List<PMLFrecuenciaVM>();

                PMLFrecuenciaVM objEquipos = new PMLFrecuenciaVM
                {
                    Accion = 1,
                    IdUsuario = idUsuario,
                    Frec_IdPlanta = idPlanta,
                    Frec_IdEquipo = idEquipo
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaFrecuencia", JsonConvert.SerializeObject(objEquipos));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaFrecuencia = dtConsulta.AsEnumerable()
                                             .Select(dataRow => new PMLFrecuenciaVM
                                             {
                                                 Frec_IdFrecuencia = Convert.ToInt32(dataRow.Field<object>("Frec_IdFrecuencia") ?? 0),
                                                 Frec_IdFrecuenciaPadre = dataRow.IsNull("Frec_IdFrecuenciaPadre") ? (int?)null : Convert.ToInt32(dataRow.Field<object>("Frec_IdFrecuenciaPadre")),
                                                 Frec_IdEquipo = Convert.ToInt32(dataRow.Field<object>("Frec_IdEquipo") ?? 0),
                                                 Frec_IdTipoProducto = Convert.ToInt32(dataRow.Field<object>("Frec_IdTipoProducto") ?? 0),
                                                 Frec_IdProducto = dataRow.Field<string>("Frec_IdProducto") ?? "",
                                                 Frec_Concentracion = Convert.ToDecimal(dataRow.Field<object>("Frec_Concentracion") ?? 0),
                                                 Frec_IdUDM = Convert.ToInt32(dataRow.Field<object>("Frec_IdUDM") ?? 0),
                                                 Frec_IdTipoLimpieza = Convert.ToInt32(dataRow.Field<object>("Frec_IdTipoLimpieza") ?? 0),
                                                 Frec_DescripcionFrecuencia = dataRow.Field<string>("Frec_DescripcionFrecuencia") ?? "",
                                                 Frec_Frecuencia = dataRow.Field<string>("Frec_Frecuencia") ?? "",
                                                 Frec_DiasSemana = dataRow.Field<string>("Frec_DiasSemana") ?? "",
                                                 Frec_DiaAplicacion = dataRow.IsNull("Frec_DiaAplicacion") ? (int?)null : Convert.ToInt32(dataRow.Field<object>("Frec_DiaAplicacion")),
                                                 Frec_MesAplicacion = dataRow.IsNull("Frec_MesAplicacion") ? (int?)null : Convert.ToInt32(dataRow.Field<object>("Frec_MesAplicacion")),
                                                 DescripcionProducto = dataRow.Field<string>("DescripcionProducto") ?? "",
                                                 DescripcionTipoProducto = dataRow.Field<string>("DescripcionTipoProducto") ?? "",
                                                 DescripcionUDM = dataRow.Field<string>("DescripcionUDM") ?? "",
                                                 DescripcionTipoLimpieza = dataRow.Field<string>("DescripcionTipoLimpieza") ?? ""

                                             }).ToList();
                }

                return listaFrecuencia;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ListaFrecuencia()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Este método nos permite consultar la lista de productos asociados a un tipo de producto
        /// JGPJ 22-04-2024
        /// </summary>
        /// <param name="idTipoProducto"></param>
        /// <returns>Nos retorna una List de ComboProductos</returns>
        public async Task<List<ComboVM>> ComboProductos(int idTipoProducto)
        {
            try
            {
                List<ComboVM> listaCombo = new List<ComboVM>();


                PMLProductosVM objProductosVM = new PMLProductosVM
                {
                    Accion = 1,
                    CustId = idErp,
                    IdTipoProducto = idTipoProducto,
                    IdUsuario = idUsuario,
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaProductos", JsonConvert.SerializeObject(objProductosVM));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaCombo = dtConsulta.AsEnumerable()
                                           .Select(dataRow => new ComboVM
                                           {
                                               TextoOpcion = dataRow.Field<string>("Parte") ?? "",
                                               ValorCadena = dataRow.Field<string>("PartNum") ?? ""
                                           }).ToList();
                }

                return listaCombo;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ComboProductos", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Este método nos permitirá gestionar las frecuencias de los equipos Insertar, Editar, Eliminar
        /// JGPJ 22/04/2024
        /// </summary>
        /// <param name="objFrecuenciaVM">Objeto de datos de PMLFrecuenciaVM</param>
        /// <returns>
        ///   La tarea devolverá un objeto que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> GestionarFrecuencia(PMLFrecuenciaVM objFrecuenciaVM)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;

                objFrecuenciaVM.IdUsuario = idUsuario;
                objFrecuenciaVM.Frec_UsuarioCrea = idUsuario;
                objFrecuenciaVM.Frec_UsuarioMod = idUsuario;
                objFrecuenciaVM.Frec_IdPlanta = idPlanta;

                resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarFrecuencia", JsonConvert.SerializeObject(objFrecuenciaVM));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ha ocurrido un error durante la operación.";
                }
                else
                {
                    estatus = true;
                    mensaje = "La operación se ha completado correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/GestionarFrecuencia", idUsuario.ToString(), "Error", "Portal");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }


        /// <summary>
        /// Este método nos permitirá consultar los datos de las areas y subáreas que se encuentren activas y que se encuentren asociadas al id de la planta
        /// JGPJ 24/04/2024
        /// </summary>
        /// <param name="IdArea">Identificador para el id del área a consultar (opcional)</param>
        /// <returns>nos retorna una List de PMLAreasVM con los datos de las areas y SubAreas</returns>
        public async Task<List<PMLAreasVM>> ListaAreasYSubAreas(int? idArea)
        {
            try
            {
                List<PMLAreasVM> listaAreasSubareas = new List<PMLAreasVM>();

                PMLAreasVM objAreasVM = new PMLAreasVM
                {
                    Accion = 2,
                    IdUsuario = idUsuario,
                    Area_IdPlanta = idPlanta,
                    Area_IdArea = idArea
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaAreas", JsonConvert.SerializeObject(objAreasVM));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaAreasSubareas = dtConsulta.AsEnumerable()
                                                 .Select(dataRow => new PMLAreasVM
                                                 {
                                                     Area_IdArea = Convert.ToInt32(dataRow.Field<object>("Area_IdArea") ?? 0),
                                                     Area_IdAreaPadre = Convert.ToInt32(dataRow.Field<object>("Area_IdArea") ?? 0),
                                                     Area_IdPlanta = Convert.ToInt32(dataRow.Field<object>("Area_IdPlanta") ?? 0),
                                                     Area_Nombre = dataRow.Field<string>("Area_Nombre") ?? "",
                                                     Area_IdResponsable = dataRow.IsNull("Area_IdResponsable") ? (int?)null : Convert.ToInt32(dataRow.Field<object>("Area_IdResponsable")),
                                                     Area_IdSupervisor = dataRow.IsNull("Area_IdSupervisor") ? (int?)null : Convert.ToInt32(dataRow.Field<object>("Area_IdSupervisor")),
                                                     NombreAreaPadre = dataRow.Field<string>("NombreAreaPadre") ?? ""
                                                 }).ToList();
                }

                return listaAreasSubareas;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ListaAreasYSubAreas()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }



        /// <summary>
        /// Este método nos permitirá gestionar las areas y sub areas de los equipos Insertar, Editar, Eliminar
        /// JGPJ 23/04/2024
        /// </summary>
        /// <param name="objFrecuenciaVM">Objeto de datos de PMLAreasVM</param>
        /// <returns>
        ///   La tarea devolverá un objeto que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> GestionarAreasYSubAreas(PMLAreasVM objAreasVM)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;

                objAreasVM.IdUsuario = idUsuario;
                objAreasVM.Area_UsuarioCrea = idUsuario;
                objAreasVM.Area_UsuarioMod = idUsuario;
                objAreasVM.Area_IdPlanta = idPlanta;

                resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarAreasYSubAreas", JsonConvert.SerializeObject(objAreasVM));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ha ocurrido un error durante la operación.";
                }
                else
                {
                    estatus = true;
                    mensaje = "La operación se ha completado correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/GestionarAreasYSubAreas", idUsuario.ToString(), "Error", "Portal");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }


        /// <summary>
        /// Este método nos permitirá consultar los datos de los colaboradores.
        /// JGPJ 24-04-2024
        /// Los códigos de puesto son:
        /// 11 - Supervisor
        /// 12 - Operador
        /// 13 - Responsable
        /// </summary>
        /// <param name="idPuesto">
        /// Si se recibe un número, la query del SP_Colaboradores filtrará por la columna Col_Puesto.
        /// De lo contrario, devolverá todos los colaboradores asociados a la planta.
        /// </param>
        /// <returns>
        /// Nos retorna una lista de objetos ComboVM.
        /// </returns>
        public async Task<List<ComboVM>> ComboColaboradores(int? idPuesto)
        {
            try
            {
                List<ComboVM> listaTipoEquipo = new List<ComboVM>();


                PMLColaboradoresVM objColaboradores = new PMLColaboradoresVM
                {
                    Accion = 4,
                    IdUsuario = idUsuario,
                    Col_IdPlanta = idPlanta,
                    Col_Puesto = idPuesto,
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaColaboradores", JsonConvert.SerializeObject(objColaboradores));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaTipoEquipo = dtConsulta.AsEnumerable()
                                                .Select(dataRow => new ComboVM
                                                {
                                                    TextoOpcion = dataRow.Field<string>("Col_Nombre") ?? "",
                                                    ValorEntero = Convert.ToInt32(dataRow.Field<object>("Col_IdColaborador")),
                                                }).ToList();
                }

                return listaTipoEquipo;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ComboColaboradores", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Este método nos permitirá consultar los datos de los programas realizados
        /// JGPJ 25/04/2024
        /// </summary>
        /// <param name="objProgramaVM">Objeto de datos de Lista Programa Maestro</param>
        /// <returns>Nos retorna una lista de PMLProgramaVM</returns>
        public async Task<List<PMLProgramaVM>> ListaProgramaMaestro(PMLProgramaVM objProgramaVM)
        {
            try
            {
                List<PMLProgramaVM> listaPrograma = new List<PMLProgramaVM>();

                objProgramaVM.IdUsuario = idUsuario;
                objProgramaVM.Pro_IdPlanta = idPlanta;


                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaPrograma", JsonConvert.SerializeObject(objProgramaVM));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaPrograma = dtConsulta.AsEnumerable()
                                              .Select(dataRow => new PMLProgramaVM
                                              {
                                                  Pro_IdPrograma = Convert.ToInt32(dataRow.Field<object>("Pro_IdPrograma") ?? 0),
                                                  Pro_FechaPrograma = dataRow.IsNull("Pro_FechaPrograma") ? (DateTime?)null : Convert.ToDateTime(dataRow.Field<object>("Pro_FechaPrograma"), CultureInfo.CurrentCulture),
                                                  Pro_IdTurno = Convert.ToInt32(dataRow.Field<object>("Pro_IdPrograma") ?? 0),
                                                  Pro_IdPlanta = Convert.ToInt32(dataRow.Field<object>("Pro_IdPlanta") ?? 0),
                                                  Pro_Estatus = dataRow.Field<string>("Pro_Estatus") ?? "",
                                                  NombreTurno = dataRow.Field<string>("NombreTurno") ?? ""
                                              }).ToList();
                }

                return listaPrograma;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ListaProgramaMaestro()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Este método nos permitirá consultar la lista de Areas
        /// GFLT 22/04/2024
        /// </summary>
        /// <returns>Nos retorna una lista de PMLAreasVM</returns>
        public async Task<List<PMLAreasVM>> BuscadorAreas()
        {
            try
            {
                List<PMLAreasVM> listaAreas = new List<PMLAreasVM>();

                PMLAreasVM objAreas = new PMLAreasVM
                {
                    Accion = 1,
                    Area_IdPlanta = idPlanta
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaAreas", JsonConvert.SerializeObject(objAreas));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaAreas = dtConsulta.AsEnumerable()
                                    .Select(dataRow => new PMLAreasVM
                                    {
                                        Area_IdArea = Convert.ToInt32(dataRow.Field<object>("Area_IdArea") ?? 0),
                                        Area_Nombre = dataRow.Field<string>("Area_Nombre")

                                    }).ToList();
                }
                return listaAreas;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ObtenerAreas", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Este método nos permitirá consultar la lista de Equipos realcionado a una Area 
        /// GFLT 22/04/2024
        /// </summary>
        /// <returns>Nos retorna una lista de PMLAreasVM</returns>
        public async Task<List<PMLEquiposVM>> BuscadorEquipos(int? area)
        {
            try
            {
                List<PMLEquiposVM> listaEquipos = new List<PMLEquiposVM>();

                PMLEquiposVM objEquipos = new PMLEquiposVM
                {
                    Equi_IdArea = area,
                    Accion = 6,
                    Equi_IdPlanta = idPlanta
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaEquipos", JsonConvert.SerializeObject(objEquipos));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaEquipos = dtConsulta.AsEnumerable()
                                    .Select(dataRow => new PMLEquiposVM
                                    {
                                        Equi_IdEquipo = Convert.ToInt32(dataRow.Field<object>("Equi_IdEquipo") ?? 0),
                                        Equi_Nombre = dataRow.Field<string>("Equi_Nombre")

                                    }).ToList();
                }
                return listaEquipos;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ObtenerAreas", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }

        public async Task<object> GestionarTurnos(PMLTurnosVM objTurnosVM)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;


                objTurnosVM.IdUsuario = idUsuario;
                objTurnosVM.Tur_UsuarioCrea = idUsuario;
                objTurnosVM.Tur_UsuarioMod = idUsuario;
                objTurnosVM.Tur_IdPlanta = idPlanta;

                resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarTurnos", JsonConvert.SerializeObject(objTurnosVM));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/GestionarEquipos", idUsuario.ToString(), "Error", "Portal");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }



        /// <summary>
        /// Método que nos permitirá obtener los Turnos 
        /// GFLT 22/04/2024 
        /// </summary>
        /// <returns>Nos retora una Lista de ComboVM</returns>
        public async Task<List<PMLTurnosVM>> ConsultarTurnos()
        {
            try
            {
                List<PMLTurnosVM> listaTurnos = new List<PMLTurnosVM>();

                PMLTurnosVM objTurnos = new PMLTurnosVM
                {
                    Accion = 0,
                    Tur_IdPlanta = idPlanta,
                    IdUsuario = idUsuario,
                };


                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaTurnos", JsonConvert.SerializeObject(objTurnos));


                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaTurnos = dtConsulta.AsEnumerable()
                                      .Select(dataRow => new PMLTurnosVM
                                      {
                                          Tur_Descripcion = dataRow.Field<string>("Tur_Descripcion") ?? "",
                                          Tur_Id = Convert.ToInt32(dataRow.Field<object>("Tur_Id") ?? 0),
                                          Tur_HoraEntrada = DateTime.TryParseExact(dataRow.Field<string>("Tur_HoraEntrada"), "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime horaEntrada) ? horaEntrada : (DateTime?)null,
                                          Tur_HoraSalida = DateTime.TryParseExact(dataRow.Field<string>("Tur_HoraSalida"), "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime horaSalida) ? horaSalida : (DateTime?)null

                                      }).ToList();
                }
                return listaTurnos;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ObtenerAreas", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }

        /// <summary>
        /// Este Método nos permite consultar las calificaciones
        /// OCA 17/04/2024
        /// </summary>
        /// <returns>Nos retorna una Lista de PMLCalificacionesVM</returns>
        public async Task<List<PMLCalificacionesVM>> ListaCalificaciones(int idColaborador)
        {
            try
            {
                List<PMLCalificacionesVM> listaColaboradores = new List<PMLCalificacionesVM>();

                PMLCalificacionesVM objEquipos = new PMLCalificacionesVM
                {
                    Accion = 0,
                    Cal_IdColaborador = idColaborador,
                    Cal_IdPlanta = idPlanta
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaCalificaciones", JsonConvert.SerializeObject(objEquipos));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaColaboradores = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new PMLCalificacionesVM
                                   {
                                       Cal_IdCalificacion = Convert.ToInt32(dataRow.Field<object>("Cal_IdCalificacion") ?? 0),
                                       Cal_IdColaborador = Convert.ToInt32(dataRow.Field<object>("Cal_IdColaborador") ?? 0),
                                       Cal_IdCurso = Convert.ToInt32(dataRow.Field<object>("Cal_IdCurso") ?? 0),
                                       Cal_NombreDoc = dataRow.Field<string>("Cal_NombreDoc"),
                                       Cal_ExtensionDoc = dataRow.Field<string>("Cal_ExtensionDoc"),
                                       Cal_Calificacion = Convert.ToDecimal(dataRow.Field<object>("Cal_Calificacion") ?? 0),
                                       Cal_FechaVigencia = Convert.ToDateTime(dataRow.Field<object>("Cal_FechaVigencia") ?? DateTime.MinValue),
                                       Cal_FechaCurso = Convert.ToDateTime(dataRow.Field<object>("Cal_FechaCurso") ?? DateTime.MinValue),
                                       NombreDelColaborador = dataRow.Field<string>("NombreDelColaborador"),
                                       NombreDelCurso = dataRow.Field<string>("NombreDelCurso")
                                   }).ToList();
                }

                return listaColaboradores;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ListaCalificaciones()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Este Método nos permite consultar las Cursos
        /// GFLT 17/04/2024
        /// </summary>
        /// <returns>Nos retorna una Lista de PMLCursosVM</returns>
        public async Task<List<PMLCursosVM>> ListaCursos()
        {
            try
            {
                List<PMLCursosVM> listaCursos = new List<PMLCursosVM>();

                PMLCursosVM objCursos = new PMLCursosVM
                {
                    Accion = 1,
                    IdUsuario = idUsuario,
                    Cur_IdPlanta = idPlanta
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaCursos", JsonConvert.SerializeObject(objCursos));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaCursos = dtConsulta.AsEnumerable()
                                    .Select(dataRow => new PMLCursosVM
                                    {
                                        Cur_IdCurso = Convert.ToInt32(dataRow.Field<object>("Cur_IdCurso") ?? 0),
                                        Cur_Nombre = dataRow.Field<string>("Cur_Nombre"),
                                        Cur_AreaNombre = dataRow.Field<string>("Cur_AreaNombre"),
                                        Cur_EquipoNombre = dataRow.Field<string>("Cur_EquipoNombre"),
                                        Cur_Objetivos = dataRow.Field<string>("Cur_Objetivos"),
                                        Cur_IdPlanta = Convert.ToInt32(dataRow.Field<object>("Cur_IdPlanta") ?? 0),
                                        Cur_IdArea = Convert.ToInt32(dataRow.Field<object>("Cur_IdArea") ?? 0),
                                        Cur_IdEquipo = Convert.ToInt32(dataRow.Field<object>("Cur_IdEquipo") ?? 0),
                                    }).ToList();
                }


                return listaCursos;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/DTConsultaCursos()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
        /// OCA 16/04/2024
        /// </summary>
        /// <param name="objCalificacion">Objeto de datos de PMLCalificacionesVM</param>
        /// <returns>
        /// Un objeto Task que representa la operación asincrónica. 
        /// La tarea devolverá un objeto anónimo que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> GestionarCalificaciones(PMLCalificacionesVM objCalificacion)
		{
			try
			{
				bool estatus = false;
				string mensaje = "";
				int resultado = 0;


                objCalificacion.IdUsuario = idUsuario;
                objCalificacion.Cal_IdUsuarioCrea = idUsuario;
                objCalificacion.Cal_IdUsuarioMod = idUsuario;
                objCalificacion.Cal_IdPlanta = idPlanta;

                resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarCalificaciones", JsonConvert.SerializeObject(objCalificacion));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/GestionarEquipos", idUsuario.ToString(), "Error", "Portal");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }



        /// <summary>
        /// Método que nos permitirá gestionar los Programas Maestro
        /// </summary>
        /// <param name="objProgramaVM">Objeto de datos de PMLProgramaVM</param>
        /// <returns>Nos retorna un objeto de datos de tipo RespuestaOperacionVM</returns>
        public async Task<RespuestaOperacionVM> GestionarProgramaMaestro(PMLProgramaVM objProgramaVM)
        {
            RespuestaOperacionVM objRespuesta = new RespuestaOperacionVM();

            try
            {

                objProgramaVM.IdUsuario = idUsuario;
                objProgramaVM.Pro_IdUsuarioCrea = idUsuario;
                objProgramaVM.Pro_IdUsuarioMod = idUsuario;
                objProgramaVM.Pro_IdPlanta = idPlanta;

                objRespuesta.Resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarProgramaMaestro", JsonConvert.SerializeObject(objProgramaVM));

                if (objRespuesta.Resultado == 0)
                {
                    objRespuesta.Estatus = false;
                    objRespuesta.Mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    objRespuesta.Estatus = true;
                    objRespuesta.Mensaje = "La ejecución se realizó correctamente.";
                }

                return objRespuesta;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/GestionarProgramaMaestro", idUsuario.ToString(), "Error", "Portal");
                objRespuesta.Estatus = false;
                objRespuesta.Mensaje = "Error en el servidor: " + ex.Message;

                return objRespuesta;
            }
        }


        /// <summary>
        /// Método para consultar la información de un programa maestro específico
        /// </summary>
        /// <param name="idPrograma">Identificador del programa a consultar</param>
        /// <returns>Nos retorna un objeto de PMLProgramaVM</returns>
        public async Task<PMLProgramaVM> ConsultarProgramaMaestroEspecifico(int idPrograma)
        {
            try
            {
                PMLProgramaVM objEquipo = new PMLProgramaVM
                {
                    Accion = 3,
                    IdUsuario = idUsuario,
                    Pro_IdPlanta = idPlanta,
                    Pro_IdPrograma = idPrograma
                };

                DataTable dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaPrograma", JsonConvert.SerializeObject(objEquipo));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    objEquipo = dtConsulta.AsEnumerable()
                                           .Select(dataRow => new PMLProgramaVM
                                           {
                                               Pro_IdPrograma = Convert.ToInt32(dataRow.Field<object>("Pro_IdPrograma") ?? 0),
                                               Pro_FechaPrograma = dataRow.IsNull("Pro_FechaPrograma") ? (DateTime?)null : Convert.ToDateTime(dataRow.Field<object>("Pro_FechaPrograma"), CultureInfo.CurrentCulture),
                                               Pro_IdTurno = Convert.ToInt32(dataRow.Field<object>("Pro_IdTurno") ?? 0),
                                               Pro_IdPlanta = Convert.ToInt32(dataRow.Field<object>("Pro_IdPlanta") ?? 0),
                                               Pro_Estatus = dataRow.Field<string>("Pro_Estatus") ?? "",
                                               NombreTurno = dataRow.Field<string>("NombreTurno") ?? ""
                                           }).FirstOrDefault();
                }
                else
                {
                    objEquipo = new PMLProgramaVM();
                }

                return objEquipo;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ConsultarDatosEquipo", idUsuario.ToString(), "Error", "Portal");
                return new PMLProgramaVM();
            }
        }


        /// <summary>
        /// Método para consultar la información de los detalles del programa
        /// </summary>
        /// <param name="objDetallesVM">Objeto de datos de PMLDetalleProgramaVM</param>
        /// <returns>Nos retorna una List de PMLDetalleProgramaVM</returns>
        public async Task<List<PMLDetalleProgramaVM>> ConsultarDetallesDelPrograma(PMLDetalleProgramaVM objDetallesVM)
        {
            try
            {
                objDetallesVM.IdUsuario = idUsuario;
                objDetallesVM.IdPlanta = idPlanta;

                List<PMLDetalleProgramaVM> listaDetalles = new List<PMLDetalleProgramaVM>();

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaDetallesPrograma", JsonConvert.SerializeObject(objDetallesVM));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaDetalles = dtConsulta.AsEnumerable()
                                             .Select(dataRow => new PMLDetalleProgramaVM
                                             {
                                                 DetPro_IdDetalle           =   Convert.ToInt32(dataRow.Field<object>("DetPro_IdDetalle") ?? 0),
                                                 DetPro_IdPrograma          =   Convert.ToInt32(dataRow.Field<object>("DetPro_IdPrograma") ?? 0),
                                                 DetPro_IdAreaPrograma      =   Convert.ToInt32(dataRow.Field<object>("DetPro_IdAreaPrograma") ?? 0),
                                                 DetPro_IdArea              =   Convert.ToInt32(dataRow.Field<object>("DetPro_IdArea") ?? 0),
                                                 NombreDelArea              =   dataRow.Field<string>("NombreDelArea") ?? "",
                                                 DetPro_IdEquipo            =   Convert.ToInt32(dataRow.Field<object>("DetPro_IdEquipo") ?? 0),
                                                 DetPro_NombreEquipo        =   dataRow.Field<string>("DetPro_NombreEquipo") ?? "",
                                                 DetPro_IdProductoBase      =   dataRow.Field<string>("DetPro_IdProductoBase") ?? "",
                                                 DetPro_IdProductoAlt1      =   dataRow.Field<string>("DetPro_IdProductoAlt1") ?? "",
                                                 DetPro_IdProductoAlt2      =   dataRow.Field<string>("DetPro_IdProductoAlt2") ?? "",
                                                 DetPro_IdColaborador       =   Convert.ToInt32(dataRow.Field<object>("DetPro_IdColaborador") ?? 0),
                                                 DetPro_NombreColaborador   =   dataRow.Field<string>("DetPro_NombreColaborador") ?? "",
                                                 DetPro_NombreProductoBase  =   dataRow.Field<string>("DetPro_NombreProductoBase") ?? "",
                                                 DetPro_NombreProductoAlt1  =   dataRow.Field<string>("DetPro_NombreProductoAlt1") ?? "",
                                                 DetPro_NombreProductoAlt2  =   dataRow.Field<string>("DetPro_NombreProductoAlt2") ?? "",
                                                 DetPro_ProductoBaseActivo  =   Convert.ToBoolean(dataRow.Field<object>("DetPro_ProductoBaseActivo") ?? false),
                                                 DetPro_ProductoAlt1Activo  =   Convert.ToBoolean(dataRow.Field<object>("DetPro_ProductoAlt1Activo") ?? false),
                                                 DetPro_ProductoAlt2Activo  =   Convert.ToBoolean(dataRow.Field<object>("DetPro_ProductoAlt2Activo") ?? false),
                                                 IdResponsable              =   Convert.ToInt32(dataRow.Field<object>("IdResponsable") ?? 0),
                                                 NombreResponsable          =   dataRow.Field<string>("NombreResponsable") ?? "",
                                                 IdSupervisor               =   Convert.ToInt32(dataRow.Field<object>("IdSupervisor") ?? 0),
                                                 NombreSupervisor           =   dataRow.Field<string>("NombreSupervisor") ?? "",
                                                 NumeroLinea                =   Convert.ToInt32(dataRow.Field<object>("NumeroLinea") ?? 0),

                                             }).ToList();
                }

                return listaDetalles;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/ConsultarDetallesDelPrograma()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }


        /// <summary>
        /// Este método nos permitirá consultar la lista de colaboradores del pograma
        /// </summary>
        /// <param name="idArea">Id del are a consultar</param>
        /// <returns>Nos retorna una List de PMLColaboradoresVM</returns>
        public async Task<List<PMLColaboradoresVM>> ListaBuscadorColaboradoresPrograma(int accion, int? idArea, int? idEquipo, int? idPuesto)
        {
            try
            {
                List<PMLColaboradoresVM> listaColaboradores = new List<PMLColaboradoresVM>();

                PMLColaboradoresVM objEquipos = new PMLColaboradoresVM
                {
                    Accion          =   accion,
                    IdArea          =   idArea,
                    IdEquipo        =   idEquipo,
                    IdUsuario       =   idUsuario,
                    Col_IdPlanta    =   idPlanta,
                    Col_Puesto      =   idPuesto
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaColaboradores", JsonConvert.SerializeObject(objEquipos));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaColaboradores = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new PMLColaboradoresVM
                                   {
                                       Col_IdColaborador    =   Convert.ToInt32(dataRow.Field<object>("Col_IdColaborador") ?? 0),
                                       Col_Nombre           =   dataRow.Field<string>("Col_Nombre"),
                                       Col_Puesto           =   Convert.ToInt32(dataRow.Field<object>("Col_Puesto") ?? 0),
                                       Col_Activo           =   Convert.ToBoolean(dataRow.Field<object>("Col_Activo") ?? false),
                                       Col_UsuarioCrea      =   Convert.ToInt32(dataRow.Field<object>("Col_UsuarioCrea") ?? 0),
                                       Col_UsuarioMod       =   Convert.ToInt32(dataRow.Field<object>("Col_UsuarioMod") ?? 0),
                                       Col_FechaCrea        =   Convert.ToDateTime(dataRow.Field<object>("Col_FechaCrea") ?? DateTime.MinValue),
                                       Col_FechaMod         =   Convert.ToDateTime(dataRow.Field<object>("Col_FechaMod") ?? DateTime.MinValue),
                                       Pue_Nombre           =   dataRow.Field<string>("NombrePuesto"),
                                       Col_IdPlanta         =   Convert.ToInt32(dataRow.Field<object>("Col_IdPlanta") ?? 0)
                                   }).ToList();
                }

                return listaColaboradores;
            }
            catch (Exception ex)
            { 
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ListaBuscadorColaboradoresPrograma", idUsuario.ToString(), "Error", "Portal");
                return new List<PMLColaboradoresVM>();
            }
        }


        /// <summary>
        /// Método que nos permite insertar los detalles del programa
        /// </summary>
        /// <param name="listaProgramasVM">Lista del objeto PMLDetalleProgramaVM</param>
        /// <returns>Nos retorna un objeto RespuestaOperacionVM</returns>
        public async Task<RespuestaOperacionVM> ListaGestionarProgramas(PMLProgramaVM objPrograma)
        {
            RespuestaOperacionVM objRespuesta = new RespuestaOperacionVM();

            try
            {
                objPrograma.IdUsuario           =   idUsuario;
                objPrograma.Pro_IdUsuarioCrea   =   idUsuario;
                objPrograma.Pro_IdUsuarioMod    =   idUsuario;
                objPrograma.Pro_IdPlanta        =   idPlanta;

                objRespuesta.Resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarProgramas", JsonConvert.SerializeObject(objPrograma));

                if (objRespuesta.Resultado == 0)
                {
                    objRespuesta.Estatus = false;
                    objRespuesta.Mensaje = "Ha ocurrido un error durante la operación.";
                }
                else
                {
                    objRespuesta.Estatus = true;
                    objRespuesta.Mensaje = "La operación se ha completado correctamente.";
                }

                return objRespuesta;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ListaGestionarProgramas", idUsuario.ToString(), "Error", "Portal");
                objRespuesta.Estatus = false;
                objRespuesta.Mensaje = "Error en el servidor: " + ex.Message;
                objRespuesta.Resultado = 0;
                return objRespuesta;
            }
        }



        /// <summary>
        /// Este método nos permite consultar los colaboradores que están calificados para brindar limpieza sobre un equipo o un área
        /// </summary>
        /// <param name="accion">Accion a ejecutar en el StoredProcedure</param>
        /// <param name="idArea">Identificador del área a consultar</param>
        /// <param name="idEquipo">Identificador del equipo</param>
        /// <returns></returns>
        public async Task<List<PMLCalificacionesVM>> ListaBuscadorColaboradorCalificado(int accion, int? idArea, int? idEquipo)
        {
            try
            {
                List<PMLCalificacionesVM> listaColaboradores = new List<PMLCalificacionesVM>();

                PMLCalificacionesVM objCalificaciones = new PMLCalificacionesVM
                {
                    Accion          =   accion,
                    IdUsuario       =   idUsuario,
                    Cal_IdPlanta    =   idPlanta,
                    IdArea          =   idArea,
                    IdEquipo        =   idEquipo,
                };


                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaCalificaciones", JsonConvert.SerializeObject(objCalificaciones));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaColaboradores = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new PMLCalificacionesVM
                                   {
                                       Cal_IdCalificacion       =   Convert.ToInt32(dataRow.Field<object>("Cal_IdCalificacion") ?? 0),
                                       Cal_IdColaborador        =   Convert.ToInt32(dataRow.Field<object>("Cal_IdColaborador") ?? 0),
                                       NombreDelColaborador     =   dataRow.Field<string>("NombreDelColaborador") ?? "", 
                                       NombreDelCurso           =   dataRow.Field<string>("NombreDelCurso") ?? "",
                                       Cal_Calificacion         =   Convert.ToDecimal(dataRow.Field<object>("Cal_Calificacion") ?? 0),
                                       Cal_FechaVigencia        =   Convert.ToDateTime(dataRow.Field<object>("Cal_FechaVigencia") ?? DateTime.MinValue),
                                   }).ToList();
                }

                return listaColaboradores;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ListaBuscadorColaboradoresPrograma", idUsuario.ToString(), "Error", "Portal");
                return new List<PMLCalificacionesVM>();
            }
        }


        /// <summary>
        /// Este método nos permitirá consultar los datos de los colaboradores asociados a un área
        /// </summary>
        /// <returns>}Nos retorna una List<PMLColaboradoresVM> </returns>
        public async Task<List<PMLColaboradoresVM>> ListaColaboradoresAreas(int? idArea, int? idPuesto)
        {
            try
            {
                List<PMLColaboradoresVM> listaColaboradores = new List<PMLColaboradoresVM>();

                PMLColaboradoresVM objEquipos = new PMLColaboradoresVM
                {
                    Accion = 8,
                    Col_IdPlanta = idPlanta,
                    Col_Puesto = idPuesto,
                    IdUsuario = idUsuario,
                    IdArea = idArea,
                    
                };


                DataTable dtConsulta = new DataTable();
                dtConsulta = await objComServApi.DTObtenerDatosApi("ApiPML/DTConsultaColaboradores", JsonConvert.SerializeObject(objEquipos));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaColaboradores = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new PMLColaboradoresVM
                                   {
                                       Col_IdColaborador = Convert.ToInt32(dataRow.Field<object>("Col_IdColaborador") ?? 0),
                                       Col_Nombre = dataRow.Field<string>("Col_Nombre"),
                                       Col_Puesto = Convert.ToInt32(dataRow.Field<object>("Col_Puesto") ?? 0),
                                       Col_FechaMod = Convert.ToDateTime(dataRow.Field<object>("Col_FechaMod") ?? DateTime.MinValue),
                                       Pue_Nombre = dataRow.Field<string>("NombrePuesto"),
                                       Col_IdPlanta = Convert.ToInt32(dataRow.Field<object>("Col_IdPlanta") ?? 0)
                                   }).ToList();
                }

                return listaColaboradores;
            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/DatosApiClientes/ListaColaboradoresAreas()", idUsuario.ToString(), "Error", "Portal");
                return null;
            }
        }



        /// <summary>
        /// Este método nos permitirá gestionar las areas y sub areas de los equipos Insertar, Editar, Eliminar
        /// JGPJ 23/04/2024
        /// </summary>
        /// <param name="objFrecuenciaVM">Objeto de datos de PMLAreasVM</param>
        /// <returns>
        ///   La tarea devolverá un objeto que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<RespuestaOperacionVM> GestionarDetallesPrograma(PMLDetalleProgramaVM objDetallesVM)
        {
            RespuestaOperacionVM objRespuesta = new RespuestaOperacionVM();

            try
            {
                objDetallesVM.IdUsuario             =       idUsuario;
                objDetallesVM.DetPro_UsuarioCrea    =       idUsuario;
                objDetallesVM.DetPro_UsuarioMod     =       idUsuario;
                objDetallesVM.IdPlanta              =       idPlanta;

                objRespuesta.Resultado = await objComServApi.GestionarCRUD("ApiPML/GestionarDetallesPrograma", JsonConvert.SerializeObject(objDetallesVM));

                if (objRespuesta.Resultado == 0)
                {
                    objRespuesta.Estatus = false;
                    objRespuesta.Mensaje = "Ha ocurrido un error durante la operación.";
                }
                else
                {
                    objRespuesta.Estatus = true;
                    objRespuesta.Mensaje = "La operación se ha completado correctamente.";
                }

                return objRespuesta;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/GestionarDetallesPrograma", idUsuario.ToString(), "Error", "Portal");
                objRespuesta.Estatus = false;
                objRespuesta.Mensaje = "Error en el servidor: " + ex.Message;
                objRespuesta.Resultado = 0;
                return objRespuesta;
            }
        }


        
        public async Task<RespuestaOperacionVM> ListaGestionarDetallesPrograma(List<PMLDetalleProgramaVM> listaDetalles)
        {
            RespuestaOperacionVM objRespuesta = new RespuestaOperacionVM();

            try
            {
                if (listaDetalles != null && listaDetalles.Count>0) {

                    foreach (var detalle in listaDetalles)
                    {
                        detalle.IdUsuario = idUsuario;
                        detalle.DetPro_UsuarioCrea = idUsuario;
                        detalle.DetPro_UsuarioMod = idUsuario;
                        detalle.IdPlanta = idPlanta;
                    }


                    objRespuesta.Resultado = await objComServApi.GestionarCRUD("ApiPML/ListaGestionarDetallesPrograma", JsonConvert.SerializeObject(listaDetalles));

                    if (objRespuesta.Resultado == 0)
                    {
                        objRespuesta.Estatus = false;
                        objRespuesta.Mensaje = "Ha ocurrido un error durante la operación.";
                    }
                    else
                    {
                        objRespuesta.Estatus = true;
                        objRespuesta.Mensaje = "La operación se ha completado correctamente.";
                    }
                }
                else
                {
                    objRespuesta.Estatus = false;
                    objRespuesta.Mensaje = "No se encontraron datos para procesar.";
                    objRespuesta.Resultado = 0;
                }


                return objRespuesta;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/GestionarDetallesPrograma", idUsuario.ToString(), "Error", "Portal");
                objRespuesta.Estatus = false;
                objRespuesta.Mensaje = "Error en el servidor: " + ex.Message;
                objRespuesta.Resultado = 0;
                return objRespuesta;
            }
        }



        public async Task<RespuestaOperacionVM> ActualizarDetallesPrograma(List<PMLDetalleProgramaVM> listaDetalles)
        {
            RespuestaOperacionVM objRespuesta = new RespuestaOperacionVM();

            try
            {
                if (listaDetalles != null && listaDetalles.Count > 0)
                {

                    foreach (var detalle in listaDetalles)
                    {
                        detalle.IdUsuario           =   idUsuario;
                        detalle.DetPro_UsuarioCrea  =   idUsuario;
                        detalle.DetPro_UsuarioMod   =   idUsuario;
                        detalle.IdPlanta            =   idPlanta;
                    }


                    objRespuesta.Resultado = await objComServApi.GestionarCRUD("ApiPML/ActualizarDetallesPrograma", JsonConvert.SerializeObject(listaDetalles));

                    if (objRespuesta.Resultado == 0)
                    {
                        objRespuesta.Estatus = false;
                        objRespuesta.Mensaje = "Ha ocurrido un error durante la operación.";
                    }
                    else
                    {
                        objRespuesta.Estatus = true;
                        objRespuesta.Mensaje = "La operación se ha completado correctamente.";
                    }
                }
                else
                {
                    objRespuesta.Estatus = false;
                    objRespuesta.Mensaje = "No se encontraron datos para procesar.";
                    objRespuesta.Resultado = 0;
                }


                return objRespuesta;

            }
            catch (Exception ex)
            {
                await objComServApi.EscribirEnBitacora(ex.Message, "PMLLogica/PMLGestorDatos/GestionarDetallesPrograma", idUsuario.ToString(), "Error", "Portal");
                objRespuesta.Estatus = false;
                objRespuesta.Mensaje = "Error en el servidor: " + ex.Message;
                objRespuesta.Resultado = 0;
                return objRespuesta;
            }
        }











    }
}
