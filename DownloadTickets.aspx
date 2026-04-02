<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DownloadTickets.aspx.cs" Inherits="SmallerWorldForChildren.DownloadTickets" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="MainContent">
        <script src="https://cdnjs.cloudflare.com/ajax/libs/sweetalert/2.1.0/sweetalert.min.js"></script>

   <%-- <script>
        document.addEventListener("DOMContentLoaded", function () {

            var emailTextbox = document.getElementById('<%= txtEmail.ClientID %>');

            emailTextbox.addEventListener("blur", function () {
                var regex = /^[a-zA-Z0-9_.-@]+$/;

                if (!regex.test(emailTextbox.value)) {
                    swal("Invalid Email", "Only valid characters allowed", "error");
                    emailTextbox.value = "";
                    emailTextbox.focus();
                }
            });

        });
    </script>--%>

    <style>
        .flex {
            display: flex;
            justify-content: space-between;
        }

        .flex-column {
            flex-direction: column;
        }

        h1 {
            color: green;
            text-align: center;
        }

        .p {
            width: 100%;
            margin: auto;
            text-align: center;
            font-size: 22px;
        }

        .pay {
            display: inline-flex;
            align-items: center;
            background-color: #423B99 !important;
            color: #fff;
            padding: 12px 20px !important;
            font-size: 15px !important;
            font-weight: bold !important;
            border: 2px solid #423B99 !important;
            border-radius: 10px;
            cursor: pointer !important;
            transition: background 0.3s ease;
        }

        .form-group {
            margin-bottom: 15px;
        }

        .center {
            text-align: center;
        }

        .action-btn-row {
            display: flex;
            justify-content: center;
            gap: 16px;
            margin-top: 8px;
            flex-wrap: wrap;
        }

        .field-row {
            padding: 4px 0;
            width: 100%;
        }

        .ticket-card {
            width: 800px;
            margin: auto;
            text-align: center;
        }

        .qr-img {
            width: 70px;
            margin-top: -120px;
            margin-left: 400px;
        }

        /* Force exact GridView header/data alignment */
        .grid-align-fix th,
        .grid-align-fix td {
            text-align: center !important;
            vertical-align: middle !important;
        }

        .grid-align-fix th {
            padding-left: 0 !important;
            padding-right: 0 !important;
        }
    </style>

    <!-- ================= FORM ================= -->
    <div class="nd_options_section">
        <div class="nd_options_section nd_options_box_sizing_border_box">
            <asp:Panel runat="server" ID="pnlEnterDetails">

                <div class="field-row">
                    <asp:Label Text="Select Event Name" runat="server" />
                    <asp:DropDownList ID="drpEventName" runat="server" CssClass="wpcf7-form-control" style="width:100%;" />
                    <asp:RequiredFieldValidator ControlToValidate="drpEventName" InitialValue="0"
                        ErrorMessage="Select event" ForeColor="Red" ValidationGroup="TicketLookup" runat="server" />
                </div>

                <div class="field-row">
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="wpcf7-form-control" placeholder="Email" style="width:100%;" />
                    <asp:RequiredFieldValidator ControlToValidate="txtEmail"
                        ErrorMessage="Enter email" ForeColor="Red" ValidationGroup="TicketLookup" runat="server" />
                    <asp:RegularExpressionValidator ControlToValidate="txtEmail"
                        ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                        ErrorMessage="Invalid email" ForeColor="Red" ValidationGroup="TicketLookup" runat="server" />
                </div>

                <asp:Panel ID="divOtp" runat="server" Visible="false">
                    <div class="field-row">
                        <asp:TextBox ID="txtOtp" runat="server" MaxLength="4" placeholder="OTP" CssClass="wpcf7-form-control" style="width:100%;" />
                        <asp:RequiredFieldValidator ControlToValidate="txtOtp"
                            ErrorMessage="Enter OTP" ForeColor="Red" ValidationGroup="OtpVerify" runat="server" />
                    </div>
                </asp:Panel>

                <div class="action-btn-row">
                    <asp:Button ID="btnSubmit" runat="server" Text="Submit"
                        CssClass="pay" ValidationGroup="TicketLookup" OnClick="btnSubmit_Click" Visible="false" style="margin-left: -92%;margin-bottom: 1%;"/>

                    <asp:Button ID="btnEnter" runat="server" Text="Verify OTP"
                        CssClass="pay" ValidationGroup="OtpVerify" OnClick="btnEnter_Click" Visible="false" style="margin-left: -89%;margin-bottom: 1%;"/>
                </div>

            </asp:Panel>
        </div>
    </div>

        <!-- ================= MESSAGE ================= -->

        <asp:Label ID="lblmsg" runat="server" Visible="false" CssClass="center" />

        <!-- ================= TICKET ================= -->

        <asp:Panel ID="PnlTicket" runat="server" Visible="false">

            <div id="Div_card" runat="server" class="ticket-card">

                <img src="Images/SmallerWorldPicFront.png" style='width:558px;display:block'/>

                <div>
                    <%--<b>Ticket ID:</b>--%>
                    <asp:Label ID="lblTicketID" runat="server" style="position:absolute;font-size:22px;font-weight:700;"/><br />
                    <%--<b>Event:</b>
                    <asp:Label ID="lblEName" runat="server" /><br />
                    <b>Date:</b>
                    <asp:Label ID="lblDate" runat="server" /><br />
                    <b>Location:</b>
                    <asp:Label ID="lblLocation" runat="server" /><br />
                    <b>Time:</b>
                    <asp:Label ID="lblStartTime" runat="server" /><br />
                    <b>Amount:</b>
                    <asp:Label ID="lblAmount" runat="server" />--%>
                </div>

                <!-- FIXED QR PATH -->
                <asp:Image ID="ImageGeneratedBarcode" runat="server"
                    CssClass="qr-img" style='position:absolute;right: 41%;top: 123%;width: 127px;height:125px;z-index:2;'
                    Visible="false" />

            </div>

        </asp:Panel>

        <!-- ================= MODAL ================= -->

        <div class="modal fade" id="myModal">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-body">
                        <div class="field-row">
                            <asp:TextBox ID="txtEmailId" runat="server"
                                placeholder="Enter Email" CssClass="wpcf7-form-control" style="width:100%;" />

                            <asp:RequiredFieldValidator ControlToValidate="txtEmailId"
                                ErrorMessage="Enter email" ForeColor="Red" ValidationGroup="SendVoucher" runat="server" />

                            <asp:RegularExpressionValidator ControlToValidate="txtEmailId"
                                ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                                ErrorMessage="Invalid email" ForeColor="Red" ValidationGroup="SendVoucher" runat="server" />
                        </div>
                    </div>

                    <div class="modal-footer">
                        <asp:Button ID="btnsend" runat="server"
                            Text="Send" CssClass="pay"
                            ValidationGroup="SendVoucher"
                            OnClick="btnsend_Click" />
                    </div>

                </div>
            </div>
        </div>
        <asp:GridView Visible="false" ID="grdShowDetails" runat="server" CssClass="grid-align-fix" AllowPaging="true" AllowSorting="true" AutoGenerateColumns="False" PageSize="10" OnPageIndexChanging="grdShowDetails_PageIndexChanging"
            OnRowCommand="grdShowDetails_RowCommand" Border="0px" BorderColor="White" ShowHeaderWhenEmpty="true" Style="width: 100%; margin-bottom: 24px;">
            <AlternatingRowStyle />
            <PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" Position="Bottom" />
            <PagerStyle CssClass="csspager" HorizontalAlign="Center" ForeColor="#000000" BackColor="Transparent" />
            <Columns>
                <asp:TemplateField HeaderText="TicketId" HeaderStyle-HorizontalAlign="Center" HeaderStyle-Width="22%">
                    <ItemTemplate>
                        <asp:Label ID="Label1" runat="server" Text='<%# Bind("TicketId") %>'></asp:Label>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Center" Width="22%" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Booking Date" HeaderStyle-HorizontalAlign="Center" HeaderStyle-Width="28%">
                    <ItemTemplate>
                        <asp:Label ID="lblTicketCreatedNo" runat="server" Text='<%# Bind("CreatedDate","{0:dd/MM/yyyy}") %>'></asp:Label>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Center" Width="28%" />
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Event Name" HeaderStyle-HorizontalAlign="Center" HeaderStyle-Width="30%">
                    <ItemTemplate>
                        <asp:Label ID="Label2" runat="server" Text='<%# Bind("EventName") %>'></asp:Label>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Center" Width="30%" />
                </asp:TemplateField>
                <%--<asp:TemplateField HeaderStyle-CssClass="emov-a-table-heading" HeaderText="Ticket Count">
                    <ItemTemplate>
                        <asp:Label ID="lblTicketCount" runat="server" Text='<%# Bind("TicketCount") %>'></asp:Label>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Center" />
                </asp:TemplateField>--%>
                <asp:TemplateField HeaderText="Action" HeaderStyle-HorizontalAlign="Center" HeaderStyle-Width="20%">
                    <ItemTemplate>
                        <div style="text-align:center;">
                            <asp:Button ID="ShowTickets" runat="server" CommandName="View" CssClass="pay" Text="View Ticket" CommandArgument="View" />
                        </div>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Center" Width="20%" />
                </asp:TemplateField>
            </Columns>
            <HeaderStyle CssClass="emov-a-home-table" HorizontalAlign="Center" />
            <RowStyle CssClass="emov-a-table-data" HorizontalAlign="Center" />
            <EditRowStyle CssClass="emov-a-table-data" />
            <EmptyDataTemplate>
                <div style="text-align: center;">
                    No records found.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
</asp:Content>
