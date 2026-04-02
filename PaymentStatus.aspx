<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PaymentStatus.aspx.cs" Inherits="SmallerWorldForChildren.PaymentStatus" %>

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
    <!--start form-->
    <div id="nd_options_shortcode_cf7_5" class="nd_options_section ">

        <div class="nd_options_section nd_options_box_sizing_border_box">
            <div class="wpcf7 no-js" id="wpcf7-f5-p15-o1" lang="en-US" dir="ltr">
                <div class="screen-reader-response">
                    <p role="status" aria-live="polite" aria-atomic="true"></p>
                    <ul></ul>
                </div>
                <div class="wpcf7-form init" aria-label="Contact form" data-status="init">
                    <asp:Label runat="server" Text="" ID="lblmsg" Visible="false"></asp:Label>
                    <br />
                    <br />
                    <asp:Panel runat="server" ID="PnlTicket" Visible="false">
                        <div id="Div_card" runat="server" class="emov-input-group emov-application-form-group emov-step-form-group" style="width: 100%; margin-left: -91px;">
                            <img src="Images/frontcard.jpg" class="card-img-top" alt="..." <%--style="height:235px;margin-left:-22px;"--%> style="width: 100%;" />
                            <div class="card-img-overlay">
                                <div style="position: relative; margin-left: 10%; margin-top: -30%;">
                                    <div class="parent1">
                                        <div class="div1" style="width: max-content;"><b>TicketID:</b> </div>
                                        <div class="div2">
                                            <asp:Label ID="lblTicketID" runat="server" class=" emov-application-status-label" Text="" ForeColor="Black" Font-Bold="true"></asp:Label>
                                        </div>
                                        <div class="div3" style="width: max-content;"><b>Event Name:</b> </div>
                                        <div class="div4">
                                            <asp:Label ID="lblEName" runat="server" class=" emov-application-status-label" Text="" ForeColor="Black" Font-Bold="true"></asp:Label>
                                        </div>
                                        <div class="div5"><b>Date:</b> </div>
                                        <div class="div6">
                                            <asp:Label ID="lblDate" runat="server" class=" emov-application-status-label" Text="" ForeColor="Black" Font-Bold="true"></asp:Label>
                                        </div>
                                        <div class="div9"><b>Time:</b></div>
                                        <div class="div10">
                                            <asp:Label ID="lblStartTime" runat="server" class=" emov-application-status-label" Text="" ForeColor="Black" Font-Bold="true"></asp:Label>
                                        </div>
                                        <div class="div7"><b>Location:</b> </div>
                                        <div class="div8" style="width: 400px;">
                                            <asp:Label ID="lblLocation" runat="server" class=" emov-application-status-label" Text="" ForeColor="Black" Font-Bold="true"></asp:Label>
                                        </div>
                                        <div class="div11"><b>Amount:</b></div>
                                        <div class="div12">
                                            <asp:Label ID="lblAmount" runat="server" class=" emov-application-status-label" Text="" ForeColor="Black" Font-Bold="true"></asp:Label>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <asp:Image ID="ImageGeneratedBarcode" runat="server" Visible="false" Style="position: relative; top: -130px; width: 68px; left: 450px;" ImageUrl="~/Images/QRImage.jpg" />

                            <asp:Label runat="server" ID="lblpgresponseid"></asp:Label>
                        </div>
                        <br />
                        <br />
                        <%--		<img src="Images/SW_NewBack.jpg" visible="false"  class="card-img-top" alt="..." style="width:100%;margin-left:20px;"/>--%>
                    </asp:Panel>
                    <div class="modal fade" id="myModal" role="dialog">
                        <div class="modal-dialog">
                            <div class="modal-content">
                                <div class="modal-body">
                                    <div class="field-row">
                                        <asp:TextBox ID="txtEmailId" runat="server"
                                            class="wpcf7-form-control wpcf7-text wpcf7-validates-as-required"
                                            aria-required="true" aria-invalid="false"
                                            Style="width: 100%;"
                                            placeholder="Enter Email Id" value="" type="text"></asp:TextBox>

                                        <asp:RequiredFieldValidator ForeColor="Red" ID="RequiredFieldEmail" ErrorMessage="Please enter email." Display="Dynamic" CssClass="reqfieldvalidate" ControlToValidate="txtEmailId" ValidationGroup="AddSignsend" runat="server" />
                                        <asp:RegularExpressionValidator ID="regexEmailValid" runat="server" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                                            ControlToValidate="txtEmailId" ForeColor="Red" CssClass="reqfieldvalidate" font-weight="normal" ValidationGroup="AddSignsend" ErrorMessage="Invalid email format"></asp:RegularExpressionValidator>
                                    </div>
                                </div>
                                <div class="modal-footer">
                                    <%--<button type="button" class="btn btn-default" data-dismiss="modal">Close</button>--%>
                                    <asp:Button class="pay" Text="Send" ValidationGroup="AddSignsend" ID="btnsend" OnClick="btnsend_Click" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>
                    <%--<input type="button" class="wpcf7-form-control has-spinner wpcf7-submit" value="Download" onclick="downloadimage()" />--%>
                    <%--<input type="hidden" id="ct_checkjs_cf7_34173cb38f07f89ddbebc2ac9128303f" name="ct_checkjs_cf7" value="0">
                    <input id="apbct__email_id__wp_contact_form_7_72757" class="apbct_special_field apbct__email_id__wp_contact_form_7" autocomplete="off" name="apbct__email_id__wp_contact_form_7_72757" type="text" value="" size="30" maxlength="200"><input id="apbct_event_id" class="apbct_special_field" name="apbct_event_id" type="hidden" value="72757"><div class="wpcf7-response-output" aria-hidden="true"></div>--%>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
