<%@ Page Title="Pago de Servicio" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="PagoEmpresa.aspx.vb" Inherits="eSolutions.Web.PagoEmpresa" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
        <div class="col-12 col-md-8 col-lg-6">

            <!-- ALERTA DE PAGO REALIZADO -->
            <asp:Panel ID="pnlResultadoPago" runat="server" Visible="false" CssClass="alert alert-success shadow-sm mb-4">
                <div class="d-flex align-items-center gap-2">
                    <i class="bi bi-check-circle-fill fs-4"></i>
                    <div>
                        <h5 class="alert-heading mb-0">¡Pago Procesado Exitosamente!</h5>
                        <p class="mb-0 small"><asp:Literal ID="litResultadoPago" runat="server" /></p>
                    </div>
                </div>
            </asp:Panel>

            <!-- TARJETA PRINCIPAL DE PAGO -->
            <div class="card shadow-sm border-0 rounded-3">
                <div class="card-header bg-white border-bottom p-4">
                    <div class="d-flex align-items-center gap-3">
                        <asp:Image ID="imgLogoEmpresa" runat="server" CssClass="rounded-circle border p-1" Width="64" Height="64" />
                        <div>
                            <span class="badge bg-primary-subtle text-primary border border-primary-subtle mb-1">
                                Canal: <asp:Literal ID="litCanal" runat="server" />
                            </span>
                            <h4 class="card-title fw-bold mb-0">
                                <asp:Literal ID="litNombreEmpresa" runat="server" />
                            </h4>
                            <span class="text-muted small">Código Institucional: #<asp:Literal ID="litIdEmpresa" runat="server" /></span>
                        </div>
                    </div>
                </div>

                <div class="card-body p-4">
                    <h5 class="fw-semibold mb-3">Datos del Comprobante / Recibo</h5>

                    <div class="mb-3">
                        <label class="form-label fw-medium text-secondary">Número de Contrato / NIS / Referencia:</label>
                        <asp:TextBox ID="txtNumeroContrato" runat="server" CssClass="form-control" placeholder="Ej: 1009823471" />
                        <asp:RequiredFieldValidator ID="rfvContrato" runat="server" ControlToValidate="txtNumeroContrato"
                            ErrorMessage="El número de contrato o referencia es requerido." CssClass="text-danger small mt-1 d-block" ValidationGroup="vgPago" />
                    </div>

                    <div class="mb-4">
                        <label class="form-label fw-medium text-secondary">Monto a Pagar ($ / Local):</label>
                        <div class="input-group">
                            <span class="input-group-text">$</span>
                            <asp:TextBox ID="txtMonto" runat="server" CssClass="form-control" Text="45.50" />
                        </div>
                        <asp:RequiredFieldValidator ID="rfvMonto" runat="server" ControlToValidate="txtMonto"
                            ErrorMessage="Debe indicar el monto a pagar." CssClass="text-danger small mt-1 d-block" ValidationGroup="vgPago" />
                    </div>

                    <div class="d-grid gap-2">
                        <asp:Button ID="btnProcesarPago" runat="server" Text="Confirmar y Realizar Pago"
                            CssClass="btn btn-primary py-2 fw-semibold" ValidationGroup="vgPago" OnClick="btnProcesarPago_Click" />
                        
                        <asp:LinkButton ID="btnRegresar" runat="server" CssClass="btn btn-outline-secondary py-2" OnClick="btnRegresar_Click">
                            <i class="bi bi-arrow-left me-1"></i> Regresar al Listado de Empresas
                        </asp:LinkButton>
                    </div>
                </div>
            </div>

            <div class="mt-4 p-3 bg-light rounded-3 border text-center small text-muted">
                <i class="bi bi-info-circle me-1"></i>
                <strong>Demostración de Sesión:</strong> Esta página valida la existencia de <code>Session("EmpresaSeleccionada")</code>. Si la sesión expira o no existe, el usuario es redirigido automáticamente a la página de catálogo.
            </div>

        </div>
    </div>
</asp:Content>
