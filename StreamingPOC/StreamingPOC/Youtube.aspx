<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Youtube.aspx.cs" Inherits="StreamingPOC.Youtube" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.8.2/jquery.min.js">
    </script>
    <script src="https://apis.google.com/js/client:platform.js?onload=start" async defer>
    </script>
    <script>
        function start() {
            gapi.load('auth2', function () {
                auth2 = gapi.auth2.init({
                    client_id: '523666141221-8jckerkfihfs2iskmoagc5hpgmrpk9f9.apps.googleusercontent.com',
                    // Scopes to request in addition to 'profile' and 'email'
                    scope: 'https://www.googleapis.com/auth/youtube'
                });
            });
        }
    </script>
</head>
<body>

    <div>
       
        <button id="signinButton">Sign in with Google</button>
        <script>
            $('#signinButton').click(function () {
                auth2.grantOfflineAccess({ 'redirect_uri': 'postmessage' }).then(signInCallback);
            });
        </script>


    </div>

 
    <script>
        function signInCallback(authResult) {
            if (authResult['code']) {

                // Hide the sign-in button now that the user is authorized, for example:
                $('#signinButton').attr('style', 'display: none');
                var code = authResult['code'];
                window.location.href = '/Callback.aspx?Code=' + code;

            } else {

            }
        }
    </script>
</body>
</html>