// Phone Number
$.validator.addMethod('phoneNumber', function (value, element, params) {
    if(params == null)
        return true;

    if(!RegExp(params['pattern']).test(value))
        return false;

    if(value < params['minLength'] || value > params['maxLength'])
        return false;

    return true;
});

$.validator.unobtrusive.adapters.add('phoneNumber', ['minLength','pattern','maxLength'], function (options) {
    options.rules['phoneNumber'] = {
        pattern: options.params['pattern'],
        minLength: options.params['minLength'],
        maxLength: options.params['maxLength']};

    options.messages['phoneNumber'] = options.message;
});


// Agreement
$.validator.addMethod('agreement', function (value, element, params) {
    return value === true;
});

$.validator.unobtrusive.adapters.add('agreement', [], function (options) {
    options.rules['agreement'] = {};
    options.messages['agreement'] = options.message;
});


// Email
$.validator.addMethod('email', function (value, element, params) {
    if(params == null)
        return true;

    if(!isEmailValid(value))
        return false;

    if(value < params['minLength'] || value > params['maxLength'])
        return false;

    return true;
});

$.validator.unobtrusive.adapters.add('email', ['minLength','maxLength'], function (options) {
    options.rules['email'] = {
        minLength: options.params['minLength'],
        maxLength: options.params['maxLength']};

    options.messages['email'] = options.message;
});

function isEmailValid(email) {
    const regexStr = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return regexStr.test(String(email).toLowerCase());
}


// Non Negative Integer
$.validator.addMethod('nonNegativeInteger', function (value, element, params) {
    return value >= 0;
});

$.validator.unobtrusive.adapters.add('nonNegativeInteger', [], function (options) {
    options.rules['nonNegativeInteger'] = {};
    options.messages['nonNegativeInteger'] = options.message;
});


// Positive Integer
$.validator.addMethod('positiveInteger', function (value, element, params) {
    return value > 0;
});

$.validator.unobtrusive.adapters.add('positiveInteger', [], function (options) {
    options.rules['positiveInteger'] = {};
    options.messages['positiveInteger'] = options.message;
});


// UserName
$.validator.addMethod('userName', function (value, element, params) {
    if(params == null)
        return true;

    if(!RegExp(params['pattern']).test(value))
        return false;

    if(value < params['minLength'] || value > params['maxLength'])
        return false;

    return true;
});

$.validator.unobtrusive.adapters.add('userName', ['minLength','pattern','maxLength'], function (options) {
    options.rules['userName'] = {
        pattern: options.params['pattern'],
        minLength: options.params['minLength'],
        maxLength: options.params['maxLength']};

    options.messages['userName'] = options.message;
});