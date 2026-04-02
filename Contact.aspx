<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="SmallerWorldForChildren.Contact" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="mainContent">
    <!-- SweetAlert -->
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
                <asp:TextBox ID="txtName" MaxLength="50" runat="server"
                    CssClass="wpcf7-form-control"
                    placeholder="Full Name"
                    onkeypress="return ValidateAlpha(event);" Style="width: 100%;"></asp:TextBox>

                <asp:RequiredFieldValidator ID="rfvName"
                    ErrorMessage="Please enter name."
                    ControlToValidate="txtName"
                    ForeColor="Red"
                    ValidationGroup="AddSign"
                    runat="server" />
            </div>

            <div class="field-row">
                <asp:TextBox ID="txtEmail" MaxLength="60" runat="server"
                    CssClass="wpcf7-form-control"
                    placeholder="Email" Style="width: 100%;"></asp:TextBox>

                <asp:RequiredFieldValidator ID="RequiredFieldEmail"
                    ErrorMessage="Please enter email."
                    ControlToValidate="txtEmail"
                    ForeColor="Red"
                    ValidationGroup="AddSign"
                    runat="server" />

                <asp:RegularExpressionValidator ID="regexEmailValid"
                    runat="server"
                    ValidationExpression="\w+([-+.'']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                    ControlToValidate="txtEmail"
                    ForeColor="Red"
                    ValidationGroup="AddSign"
                    ErrorMessage="Invalid email format" />
            </div>

            <div class="field-row">
                <asp:TextBox ID="txtPhoneNo" runat="server" MaxLength="15"
                    CssClass="wpcf7-form-control"
                    placeholder="Phone Number"
                    onkeypress="return numeric(event)"
                    onkeyup="this.value = this.value.replace(/\D/g,'')" Style="width: 100%;">
                </asp:TextBox>

                <asp:RequiredFieldValidator ID="RequiredFieldValidator51"
                    ErrorMessage="Please enter mobile number."
                    ControlToValidate="txtPhoneNo"
                    ForeColor="Red"
                    ValidationGroup="AddSign"
                    runat="server" />
            </div>

            <div class="field-row">
                <asp:TextBox ID="txtDescription" MaxLength="250" runat="server"
                    TextMode="MultiLine"
                    CssClass="wpcf7-form-control"
                    placeholder="Message" Style="width: 100%;">
                </asp:TextBox>

                <asp:RequiredFieldValidator ID="rfvDescription"
                    ErrorMessage="Please enter message."
                    ControlToValidate="txtDescription"
                    ForeColor="Red"
                    ValidationGroup="AddSign"
                    runat="server" />
            </div>

            <div class="field-row">
                <asp:Button ID="btnSubmit" runat="server"
                    Text="Submit"
                    CssClass="pay"
                    Style="margin-bottom: 1%;"
                    ValidationGroup="AddSign"
                    OnClick="btnSubmit_Click" />
            </div>

            <asp:Label ID="lblmsg" runat="server" Visible="false"></asp:Label>
        </div>
    </div>

</asp:Content>
