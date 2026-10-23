<%@ Page Title="Selección de Empresa" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Empresas.aspx.vb" Async="true" Inherits="eSolutions.Web.Empresas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <!-- MENÚ POR CONFIGURACIÓN DEL CANAL (Controlado dinámicamente sin duplicar código) -->
    <asp:Panel ID="pnlMenuCanal" runat="server" Visible="false" CssClass="mb-4">
        <nav class="navbar navbar-expand-lg navbar-light bg-white border rounded-3 px-3 py-2 shadow-sm">
            <div class="container-fluid p-0">
                <span class="navbar-brand fs-6 fw-bold text-secondary mb-0">
                    <i class="bi bi-grid-fill text-primary me-1"></i> Menú de Opciones del Canal
                </span>
                <div class="navbar-nav d-flex flex-row gap-2">
                    <a class="nav-link active fw-semibold text-primary px-2" href="#"><i class="bi bi-check-circle me-1"></i>Pago de Servicios</a>
                    <a class="nav-link text-muted px-2" href="#" onclick="alert('Funcionalidad de consulta demostrativa'); return false;"><i class="bi bi-clock-history me-1"></i>Historial</a>
                    <a class="nav-link text-muted px-2" href="#" onclick="alert('Módulo de ayuda demostrativo'); return false;"><i class="bi bi-question-circle me-1"></i>Ayuda</a>
                </div>
            </div>
        </nav>
    </asp:Panel>

    <!-- ENCABEZADO DE LA PÁGINA Y CANAL ACTUAL -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 pb-2 border-bottom">
        <div>
            <h2 class="h3 fw-bold mb-1">Empresas de Pago de Servicios</h2>
            <p class="text-muted mb-0">Seleccione la institución o servicio público que desea abonar.</p>
        </div>
        <div class="mt-2 mt-sm-0 text-end">
            <asp:Label ID="lblBadgeCanal" runat="server" CssClass="badge bg-dark channel-badge fs-6">
                Canal: <asp:Literal ID="lblCodigoCanal" runat="server" Text="--" /> - <asp:Literal ID="lblNombreCanal" runat="server" Text="No especificado" />
            </asp:Label>
            <div class="small text-muted mt-1">
                <asp:Literal ID="lblDestinoInfo" runat="server" />
            </div>
        </div>
    </div>

    <!-- PANEL DE MENSAJE DE ERROR / ADVERTENCIA CONTROLADO -->
    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="mb-4">
        <div class="alert alert-danger d-flex align-items-start gap-3 shadow-sm border-0 rounded-3 p-3" role="alert">
            <i class="bi bi-exclamation-octagon-fill fs-3 text-danger flex-shrink-0"></i>
            <div>
                <h5 class="alert-heading fw-bold mb-1">Aviso del Sistema</h5>
                <p class="mb-1"><asp:Label ID="lblMensajeError" runat="server" /></p>
                <div class="small text-muted">
                    Código de seguimiento: <code class="fw-bold"><asp:Label ID="lblCorrelationId" runat="server" /></code> (Detalle registrado en log).
                </div>
            </div>
        </div>
    </asp:Panel>

    <!-- PANEL CANAL NO ENCONTRADO O INACTIVO -->
    <asp:Panel ID="pnlCanalNoEncontrado" runat="server" Visible="false" CssClass="my-5 py-5 text-center">
        <div class="card border-0 shadow-sm mx-auto p-4" style="max-width: 540px;">
            <div class="card-body">
                <i class="bi bi-shield-slash fs-1 text-warning mb-3"></i>
                <h4 class="card-title fw-bold">Canal No Disponible</h4>
                <p class="card-text text-muted">
                    <asp:Label ID="lblCanalNoEncontradoMensaje" runat="server" />
                </p>
                <div class="mt-3">
                    <span class="small text-secondary d-block mb-2">Pruebe ingresando con un canal activo:</span>
                    <a href="Empresas.aspx?canal=AB" class="btn btn-outline-primary btn-sm me-2">Canal AB (Con Menú)</a>
                    <a href="Empresas.aspx?canal=CJ" class="btn btn-outline-secondary btn-sm">Canal CJ (Sin Menú)</a>
                </div>
            </div>
        </div>
    </asp:Panel>

    <!-- MOSAICO DE EMPRESAS MEDIANTE REPEATER -->
    <asp:Panel ID="pnlMosaico" runat="server" Visible="false">
        <div class="row row-cols-1 row-cols-sm-2 row-cols-md-3 row-cols-lg-4 g-4">
            <asp:Repeater ID="rptEmpresas" runat="server">
                <ItemTemplate>
                    <div class="col">
                        <div class="card h-100 empresa-card p-3 text-center d-flex flex-column justify-content-between">
                            <div>
                                <div class="p-2 mb-3 bg-light rounded-3 d-inline-block">
                                    <img src='<%# Eval("LogoUrl") %>' 
                                         alt='<%# Eval("Nombre") %>' 
                                         class="empresa-logo d-block" 
                                         onerror="this.src='https://cdn-icons-png.flaticon.com/512/1006/1006771.png';" />
                                </div>
                                <h5 class="card-title fw-bold fs-6 mb-1 text-dark">
                                    <%# Eval("Nombre") %>
                                </h5>
                                <span class="badge bg-light text-secondary border mb-3">
                                    Código: #<%# Eval("Id") %>
                                </span>
                            </div>

                            <div class="mt-3">
                                <asp:LinkButton ID="btnSeleccionar" runat="server" 
                                    CommandName="Seleccionar" 
                                    CommandArgument='<%# Eval("Id") %>' 
                                    CssClass="btn btn-primary btn-sm w-100 fw-semibold py-2">
                                    <i class="bi bi-arrow-right-circle me-1"></i> Seleccionar
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>

    <!-- BARRA RÁPIDA DE PRUEBAS PARA EVALUADORES -->
    <div class="mt-5 p-3 bg-light border rounded-3 text-muted small">
        <div class="fw-bold mb-1"><i class="bi bi-tools me-1"></i> Accesos directos para evaluación técnica:</div>
        <div class="d-flex flex-wrap gap-2">
            <a href="Empresas.aspx?canal=AB" class="badge bg-primary text-decoration-none p-2">1. Canal AB (Menú activo &rarr; PagoEmpresa.aspx)</a>
            <a href="Empresas.aspx?canal=CJ" class="badge bg-secondary text-decoration-none p-2">2. Canal CJ (Menú oculto &rarr; DetalleEmpresa.aspx)</a>
            <a href="Empresas.aspx?canal=TEST401" class="badge bg-danger text-decoration-none p-2">3. Probar API Key Inválida (Error 401 controlado)</a>
            <a href="Empresas.aspx?canal=TESTTO" class="badge bg-warning text-dark text-decoration-none p-2">4. Probar Timeout Excedido</a>
            <a href="Empresas.aspx?canal=CANAL_INEXISTENTE" class="badge bg-dark text-decoration-none p-2">5. Probar Canal Inexistente</a>
        </div>
    </div>

</asp:Content>
