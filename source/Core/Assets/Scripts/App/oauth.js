﻿function OAuthClient(store) {
    this.store = store || window.localStorage;
}

OAuthClient.prototype.makeImplicitRequest = function (authorizeUrl, clientid, callback, scope) {
    var request = this.createImplicitRequest(authorizeUrl, clientid, callback, scope);

    window.location = request.url;
};

OAuthClient.prototype.createImplicitRequest = function (authorizeUrl, clientid, callback, scope) {
    var state = (Date.now() + Math.random()) * Math.random();
    state = state.toString().replace(".", "");

    var url =
        authorizeUrl + "?" +
        "client_id=" + encodeURIComponent(clientid) + "&" +
        "response_type=token&" +
        "redirect_uri=" + encodeURIComponent(callback) + "&" +
        "state=" + encodeURIComponent(state);

    if (scope) {
        url += "&scope=" + encodeURIComponent(scope);
    }

    this.store.setItem("OAuthClient.state", state);

    return {
        state:state,
        url:url
    };
};

OAuthClient.prototype.parseResult = function (queryString) {
    queryString = queryString || location.hash;

    var idx = queryString.indexOf("#");
    if (idx > 0) {
        queryString = queryString.substr(idx + 1);
    }

    var params = {},
        regex = /([^&=]+)=([^&]*)/g,
        m;

    while (m = regex.exec(queryString)) {
        params[decodeURIComponent(m[1])] = decodeURIComponent(m[2]);
    }

    for (var prop in params) {
        return params;
    }
};

OAuthClient.prototype.readImplicitResult = function (queryString) {
    var result = OAuthClient.prototype.parseResult(queryString);
    if (!result) {
        return {
            error: "No OAuth Response"
        }
    }

    if (result.error) {
        return {
            error: result.error
        }
    }

    var state = this.store.getItem("OAuthClient.state");
    this.store.removeItem("OAuthClient.state");

    if (!state || result.state !== state) {
        return {
            error: "Invalid State"
        }
    }

    var token = result.access_token;
    if (!token) {
        return {
            error: "No Access Token"
        }
    }

    var expires_in = result.expires_in;
    if (!expires_in) {
        return {
            error: "No Token Expiration"
        }
    }

    return {
        access_token: token,
        expires_in: expires_in
    };
};

function Token(access_token, expires_at) {
    this.access_token = access_token;
    this.expires_at = parseInt(expires_at);

    Object.defineProperty(this, "expired", {
        get: function () {
            var now = parseInt(Date.now() / 1000);
            return this.expires_at < now;
        }
    });

    Object.defineProperty(this, "expires_in", {
        get: function () {
            var now = parseInt(Date.now() / 1000);
            return this.expires_at - now;
        }
    });
};

Token.fromOAuthResponse = function (response) {
    if (response.error) {
        return new Token(null, 0);
    }

    var now = parseInt(Date.now() / 1000);
    var expires_at = now + parseInt(response.expires_in);
    return new Token(response.access_token, expires_at);
};

Token.fromJSON = function (json) {
    if (json) {
        var obj = JSON.parse(json);
        return new Token(obj.access_token, obj.expires_at);
    }
    return new Token(null, 0);
};

Token.prototype.toJSON = function () {
    return JSON.stringify({
        access_token: this.access_token,
        expires_at: this.expires_at
    });
};
