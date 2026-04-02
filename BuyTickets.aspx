<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BuyTickets.aspx.cs" Inherits="SmallerWorldForChildren.BuyTickets" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="Server">

    <script src="https://cdnjs.cloudflare.com/ajax/libs/sweetalert/2.1.0/sweetalert.min.js"></script>

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

        .field-row {
            padding: 4px 0;
            width: 100%;
        }
    </style>


    <div class="nd_options_section">
        <div class="nd_options_section nd_options_box_sizing_border_box">
            <div class="field-row">
                <asp:Label runat="server" Text="Select Event Name" ID="lblEventName"></asp:Label>
                <asp:DropDownList runat="server" ID="drpEventName" AutoPostBack="true"
                    OnSelectedIndexChanged="drpEventName_SelectedIndexChanged"
                    class="wpcf7-form-control" Style="width: 100%;">
                </asp:DropDownList>
            </div>
            <p>Price Per Ticket:</p>
            <asp:Label runat="server" ID="lblPricePerticket"></asp:Label>
            <div style="display: none">
                <p>Available Tickets:</p>
                <asp:Label runat="server" ID="lblBalancedTicket"></asp:Label>
            </div>
            <asp:Panel runat="server" ID="pnlEnterDetails" Visible="false">
                <div class="field-row">
                    <asp:TextBox ID="txtName" runat="server" MaxLength="50"
                        class="wpcf7-form-control"
                        placeholder="Name"
                        onkeypress="return ValidateAlpha(event);" Style="width: 100%;"></asp:TextBox>
                    <asp:RequiredFieldValidator
                        ID="rfvName"
                        ControlToValidate="txtName"
                        ErrorMessage="Please enter name."
                        ValidationGroup="AddSign"
                        ForeColor="Red"
                        runat="server" />
                </div>
                <div class="field-row">
                    <asp:TextBox ID="txtEmail"
                        runat="server"
                        MaxLength="60"
                        class="wpcf7-form-control"
                        placeholder="Email" Style="width: 100%;"></asp:TextBox>
                    <asp:RequiredFieldValidator
                        ID="RequiredFieldEmail"
                        ControlToValidate="txtEmail"
                        ErrorMessage="Please enter email."
                        ValidationGroup="AddSign"
                        ForeColor="Red"
                        runat="server" />
                    <asp:RegularExpressionValidator
                        ID="regexEmailValid"
                        ControlToValidate="txtEmail"
                        ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                        ErrorMessage="Invalid email format"
                        ValidationGroup="AddSign"
                        ForeColor="Red"
                        runat="server" />
                </div>
                <div class="field-row">
                    <asp:DropDownList runat="server" ID="ddlCountryCode" Width="20%"></asp:DropDownList>
                    <asp:TextBox
                        ID="txtPhoneNo"
                        runat="server"
                        Width="79%"
                        MaxLength="10"
                        placeholder="Phone Number"
                        onkeypress="return numeric(event)"
                        onkeyup="if (/\D/g.test(this.value)) this.value = this.value.replace(/\D/g,'')">
                    </asp:TextBox>
                    <asp:RegularExpressionValidator
                        ID="RegularExpressionValidator1"
                        ControlToValidate="txtPhoneNo"
                        ValidationExpression="^\d{10}$"
                        ErrorMessage="Please enter 10 digit mobile number"
                        ValidationGroup="AddSign"
                        ForeColor="Red"
                        runat="server" />
                    <asp:RequiredFieldValidator
                        ID="RequiredFieldValidator51"
                        ControlToValidate="txtPhoneNo"
                        ErrorMessage="Please enter mobile number."
                        ValidationGroup="AddSign"
                        ForeColor="Red"
                        runat="server" />
                </div>
                <div class="field-row">
                    <asp:DropDownList
                        runat="server"
                        ID="ddlNationality"
                        class="wpcf7-form-control" Style="width: 100%;">
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator
                        ID="RequiredFieldValidator54"
                        ControlToValidate="ddlNationality"
                        InitialValue="0"
                        ErrorMessage="Please enter nationality."
                        ValidationGroup="AddSign"
                        ForeColor="Red"
                        runat="server" />
                </div>
                <div class="field-row">
                    <asp:TextBox
                        ID="txtNoOfTickets"
                        runat="server"
                        MaxLength="3"
                        placeholder="No. Of Tickets"
                        onkeypress="return numeric(event)" Style="width: 100%;">
                    </asp:TextBox>
                    <asp:RequiredFieldValidator
                        ID="RequiredFieldValidator1"
                        ControlToValidate="txtNoOfTickets"
                        ErrorMessage="Please enter No. Of Tickets"
                        ValidationGroup="AddSign"
                        ForeColor="Red"
                        runat="server" />
                    <asp:RangeValidator
                        ID="RangeValidator1"
                        ControlToValidate="txtNoOfTickets"
                        MinimumValue="1"
                        MaximumValue="20"
                        Type="Integer"
                        ErrorMessage="Please enter a value between 1 and 20."
                        ValidationGroup="AddSign"
                        ForeColor="Red"
                        runat="server" />
                </div>
                <span data-name="pgname">
                    <asp:DropDownList
                        runat="server"
                        ID="ddlPgName"
                        class="wpcf7-form-control" Style="width: 100%;">
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator
                        ID="RequiredFieldValidator2"
                        ControlToValidate="ddlPgName"
                        InitialValue="0"
                        ErrorMessage="Please select payment gateway."
                        ValidationGroup="AddSign"
                        ForeColor="Red"
                        runat="server" />
                </span>
                <br />
                <asp:Button
                    runat="server"
                    ID="btnSubmit"
                    Text="Pay"
                    ValidationGroup="AddSign"
                    OnClick="btnSubmit_Click"
                    CssClass="pay" style="margin-bottom: 1%;"/>
                <br />
                <asp:Label runat="server" ID="lblmsg"></asp:Label>
            </asp:Panel>
        </div>
    </div>
</asp:Content>
