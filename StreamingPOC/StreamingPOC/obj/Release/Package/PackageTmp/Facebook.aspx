<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Facebook.aspx.cs" Inherits="StreamingPOC.Facebook" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<div id="fb-root"></div>
<script>    (function (d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = "//connect.facebook.net/en_US/sdk.js#xfbml=1&version=v2.8&appId=383740905299171";
        fjs.parentNode.insertBefore(js, fjs);
    } (document, 'script', 'facebook-jssdk'));

    var accessToken;

    function test() {

        FB.getLoginStatus(function (response) {
            if (response.status === 'connected') {
                //alert("test");
                accessToken=response.authResponse.accessToken;
                //console.log(response.authResponse.accessToken);

            }
        });



    }

    function Generate() {
        FB.api(
    "/me/live_videos?access_token=" + accessToken,"POST",
    function (response) {
        if (response && !response.error) {
           // alert(response.stream_url);
           // document.write(response.stream_url);
            document.getElementById("streamId").innerHTML = response.stream_url;
        }
    }
    );
    }
    
 </script>
    <form id="form1" runat="server">
    <div>
    <div class="fb-login-button" data-max-rows="1" onlogin="test();" data-size="medium" data-show-faces="false" data-auto-logout-link="false"></div>
    
    <input type="button" onclick="Generate();" value="GenerateStreamURL" />
    <label id="streamId"></label>
    </div>
    </form>
</body>
</html>