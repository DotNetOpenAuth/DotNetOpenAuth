//!----------------------------------------------------------
//! Copyright (C) Microsoft Corporation. All rights reserved.
//!----------------------------------------------------------
//! MicrosoftMvcAjax.js

Type.registerNamespace('Sys.Mvc');

////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.AjaxOptions

Sys.Mvc.$create_AjaxOptions = function Sys_Mvc_AjaxOptions() { return {}; }


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.InsertionMode

Sys.Mvc.InsertionMode = function() { 
    /// <field name="replace" type="Number" integer="true" static="true">
    /// </field>
    /// <field name="insertBefore" type="Number" integer="true" static="true">
    /// </field>
    /// <field name="insertAfter" type="Number" integer="true" static="true">
    /// </field>
};
Sys.Mvc.InsertionMode.prototype = {
    replace: 0, 
    insertBefore: 1, 
    insertAfter: 2
}
Sys.Mvc.InsertionMode.registerEnum('Sys.Mvc.InsertionMode', false);


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.JsonValidationField

Sys.Mvc.$create_JsonValidationField = function Sys_Mvc_JsonValidationField() { return {}; }


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.JsonValidationOptions

Sys.Mvc.$create_JsonValidationOptions = function Sys_Mvc_JsonValidationOptions() { return {}; }


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.JsonValidationRule

Sys.Mvc.$create_JsonValidationRule = function Sys_Mvc_JsonValidationRule() { return {}; }


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.AjaxContext

Sys.Mvc.AjaxContext = function Sys_Mvc_AjaxContext(request, updateTarget, loadingElement, insertionMode) {
    /// <param name="request" type="Sys.Net.WebRequest">
    /// </param>
    /// <param name="updateTarget" type="Object" domElement="true">
    /// </param>
    /// <param name="loadingElement" type="Object" domElement="true">
    /// </param>
    /// <param name="insertionMode" type="Sys.Mvc.InsertionMode">
    /// </param>
    /// <field name="_insertionMode" type="Sys.Mvc.InsertionMode">
    /// </field>
    /// <field name="_loadingElement" type="Object" domElement="true">
    /// </field>
    /// <field name="_response" type="Sys.Net.WebRequestExecutor">
    /// </field>
    /// <field name="_request" type="Sys.Net.WebRequest">
    /// </field>
    /// <field name="_updateTarget" type="Object" domElement="true">
    /// </field>
    this._request = request;
    this._updateTarget = updateTarget;
    this._loadingElement = loadingElement;
    this._insertionMode = insertionMode;
}
Sys.Mvc.AjaxContext.prototype = {
    _insertionMode: 0,
    _loadingElement: null,
    _response: null,
    _request: null,
    _updateTarget: null,
    
    get_data: function Sys_Mvc_AjaxContext$get_data() {
        /// <value type="String"></value>
        if (this._response) {
            return this._response.get_responseData();
        }
        else {
            return null;
        }
    },
    
    get_insertionMode: function Sys_Mvc_AjaxContext$get_insertionMode() {
        /// <value type="Sys.Mvc.InsertionMode"></value>
        return this._insertionMode;
    },
    
    get_loadingElement: function Sys_Mvc_AjaxContext$get_loadingElement() {
        /// <value type="Object" domElement="true"></value>
        return this._loadingElement;
    },
    
    get_object: function Sys_Mvc_AjaxContext$get_object() {
        /// <value type="Object"></value>
        var executor = this.get_response();
        return (executor) ? executor.get_object() : null;
    },
    
    get_response: function Sys_Mvc_AjaxContext$get_response() {
        /// <value type="Sys.Net.WebRequestExecutor"></value>
        return this._response;
    },
    set_response: function Sys_Mvc_AjaxContext$set_response(value) {
        /// <value type="Sys.Net.WebRequestExecutor"></value>
        this._response = value;
        return value;
    },
    
    get_request: function Sys_Mvc_AjaxContext$get_request() {
        /// <value type="Sys.Net.WebRequest"></value>
        return this._request;
    },
    
    get_updateTarget: function Sys_Mvc_AjaxContext$get_updateTarget() {
        /// <value type="Object" domElement="true"></value>
        return this._updateTarget;
    }
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.AsyncHyperlink

Sys.Mvc.AsyncHyperlink = function Sys_Mvc_AsyncHyperlink() {
}
Sys.Mvc.AsyncHyperlink.handleClick = function Sys_Mvc_AsyncHyperlink$handleClick(anchor, evt, ajaxOptions) {
    /// <param name="anchor" type="Object" domElement="true">
    /// </param>
    /// <param name="evt" type="Sys.UI.DomEvent">
    /// </param>
    /// <param name="ajaxOptions" type="Sys.Mvc.AjaxOptions">
    /// </param>
    evt.preventDefault();
    Sys.Mvc.MvcHelpers._asyncRequest(anchor.href, 'post', '', anchor, ajaxOptions);
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.FieldValidation

Sys.Mvc.FieldValidation = function Sys_Mvc_FieldValidation(formValidation, fieldElements, validationMessageElement, replaceValidationMessageContents) {
    /// <param name="formValidation" type="Sys.Mvc.FormValidation">
    /// </param>
    /// <param name="fieldElements" type="Array" elementType="Object" elementDomElement="true">
    /// </param>
    /// <param name="validationMessageElement" type="Object" domElement="true">
    /// </param>
    /// <param name="replaceValidationMessageContents" type="Boolean">
    /// </param>
    /// <field name="_hasTextChangedTag" type="String" static="true">
    /// </field>
    /// <field name="_hasValidationFiredTag" type="String" static="true">
    /// </field>
    /// <field name="_inputElementErrorCss" type="String" static="true">
    /// </field>
    /// <field name="_inputElementValidCss" type="String" static="true">
    /// </field>
    /// <field name="_validationMessageErrorCss" type="String" static="true">
    /// </field>
    /// <field name="_validationMessageValidCss" type="String" static="true">
    /// </field>
    /// <field name="_onBlurHandler" type="Sys.UI.DomEventHandler">
    /// </field>
    /// <field name="_onChangeHandler" type="Sys.UI.DomEventHandler">
    /// </field>
    /// <field name="_onInputHandler" type="Sys.UI.DomEventHandler">
    /// </field>
    /// <field name="_onPropertyChangeHandler" type="Sys.UI.DomEventHandler">
    /// </field>
    /// <field name="_errors" type="Array">
    /// </field>
    /// <field name="_fieldElements" type="Array" elementType="Object" elementDomElement="true">
    /// </field>
    /// <field name="_formValidation" type="Sys.Mvc.FormValidation">
    /// </field>
    /// <field name="_replaceValidationMessageContents" type="Boolean">
    /// </field>
    /// <field name="_validationMessageElement" type="Object" domElement="true">
    /// </field>
    /// <field name="_validators" type="Array">
    /// </field>
    this._errors = [];
    this._validators = [];
    this._formValidation = formValidation;
    this._fieldElements = fieldElements;
    this._validationMessageElement = validationMessageElement;
    this._replaceValidationMessageContents = replaceValidationMessageContents;
    this._onBlurHandler = Function.createDelegate(this, this._element_OnBlur);
    this._onChangeHandler = Function.createDelegate(this, this._element_OnChange);
    this._onInputHandler = Function.createDelegate(this, this._element_OnInput);
    this._onPropertyChangeHandler = Function.createDelegate(this, this._element_OnPropertyChange);
}
Sys.Mvc.FieldValidation.prototype = {
    _onBlurHandler: null,
    _onChangeHandler: null,
    _onInputHandler: null,
    _onPropertyChangeHandler: null,
    _fieldElements: null,
    _formValidation: null,
    _replaceValidationMessageContents: false,
    _validationMessageElement: null,
    
    addError: function Sys_Mvc_FieldValidation$addError(message) {
        /// <param name="message" type="String">
        /// </param>
        this.addErrors([ message ]);
    },
    
    addErrors: function Sys_Mvc_FieldValidation$addErrors(messages) {
        /// <param name="messages" type="Array" elementType="String">
        /// </param>
        if (!Sys.Mvc._validationUtil.arrayIsNullOrEmpty(messages)) {
            Array.addRange(this._errors, messages);
            this._onErrorCountChanged();
        }
    },
    
    addValidator: function Sys_Mvc_FieldValidation$addValidator(validator) {
        /// <param name="validator" type="Sys.Mvc.Validator">
        /// </param>
        Array.add(this._validators, validator);
    },
    
    disableDynamicValidation: function Sys_Mvc_FieldValidation$disableDynamicValidation() {
        for (var i = 0; i < this._fieldElements.length; i++) {
            var fieldElement = this._fieldElements[i];
            if (Sys.Mvc._validationUtil.elementSupportsEvent(fieldElement, 'onpropertychange')) {
                Sys.UI.DomEvent.removeHandler(fieldElement, 'propertychange', this._onPropertyChangeHandler);
            }
            else {
                Sys.UI.DomEvent.removeHandler(fieldElement, 'input', this._onInputHandler);
            }
            Sys.UI.DomEvent.removeHandler(fieldElement, 'change', this._onChangeHandler);
            Sys.UI.DomEvent.removeHandler(fieldElement, 'blur', this._onBlurHandler);
        }
    },
    
    _displayError: function Sys_Mvc_FieldValidation$_displayError() {
        if (this._validationMessageElement) {
            if (this._replaceValidationMessageContents) {
                Sys.Mvc._validationUtil.setInnerText(this._validationMessageElement, this._errors[0]);
            }
            Sys.UI.DomElement.removeCssClass(this._validationMessageElement, Sys.Mvc.FieldValidation._validationMessageValidCss);
            Sys.UI.DomElement.addCssClass(this._validationMessageElement, Sys.Mvc.FieldValidation._validationMessageErrorCss);
        }
        for (var i = 0; i < this._fieldElements.length; i++) {
            var fieldElement = this._fieldElements[i];
            Sys.UI.DomElement.removeCssClass(fieldElement, Sys.Mvc.FieldValidation._inputElementValidCss);
            Sys.UI.DomElement.addCssClass(fieldElement, Sys.Mvc.FieldValidation._inputElementErrorCss);
        }
    },
    
    _displaySuccess: function Sys_Mvc_FieldValidation$_displaySuccess() {
        if (this._validationMessageElement) {
            if (this._replaceValidationMessageContents) {
                Sys.Mvc._validationUtil.setInnerText(this._validationMessageElement, '');
            }
            Sys.UI.DomElement.removeCssClass(this._validationMessageElement, Sys.Mvc.FieldValidation._validationMessageErrorCss);
            Sys.UI.DomElement.addCssClass(this._validationMessageElement, Sys.Mvc.FieldValidation._validationMessageValidCss);
        }
        for (var i = 0; i < this._fieldElements.length; i++) {
            var fieldElement = this._fieldElements[i];
            Sys.UI.DomElement.removeCssClass(fieldElement, Sys.Mvc.FieldValidation._inputElementErrorCss);
            Sys.UI.DomElement.addCssClass(fieldElement, Sys.Mvc.FieldValidation._inputElementValidCss);
        }
    },
    
    _element_OnInput: function Sys_Mvc_FieldValidation$_element_OnInput(e) {
        /// <param name="e" type="Sys.UI.DomEvent">
        /// </param>
        e.target[Sys.Mvc.FieldValidation._hasTextChangedTag] = true;
        if (e.target[Sys.Mvc.FieldValidation._hasValidationFiredTag]) {
            this.validate();
        }
    },
    
    _element_OnBlur: function Sys_Mvc_FieldValidation$_element_OnBlur(e) {
        /// <param name="e" type="Sys.UI.DomEvent">
        /// </param>
        if (e.target[Sys.Mvc.FieldValidation._hasTextChangedTag] || e.target[Sys.Mvc.FieldValidation._hasValidationFiredTag]) {
            this.validate();
        }
    },
    
    _element_OnChange: function Sys_Mvc_FieldValidation$_element_OnChange(e) {
        /// <param name="e" type="Sys.UI.DomEvent">
        /// </param>
        e.target[Sys.Mvc.FieldValidation._hasTextChangedTag] = true;
    },
    
    _element_OnPropertyChange: function Sys_Mvc_FieldValidation$_element_OnPropertyChange(e) {
        /// <param name="e" type="Sys.UI.DomEvent">
        /// </param>
        if (e.rawEvent.propertyName === 'value') {
            e.target[Sys.Mvc.FieldValidation._hasTextChangedTag] = true;
            if (e.target[Sys.Mvc.FieldValidation._hasValidationFiredTag]) {
                this.validate();
            }
        }
    },
    
    enableDynamicValidation: function Sys_Mvc_FieldValidation$enableDynamicValidation() {
        for (var i = 0; i < this._fieldElements.length; i++) {
            var fieldElement = this._fieldElements[i];
            if (Sys.Mvc._validationUtil.elementSupportsEvent(fieldElement, 'onpropertychange')) {
                Sys.UI.DomEvent.addHandler(fieldElement, 'propertychange', this._onPropertyChangeHandler);
            }
            else {
                Sys.UI.DomEvent.addHandler(fieldElement, 'input', this._onInputHandler);
            }
            Sys.UI.DomEvent.addHandler(fieldElement, 'change', this._onChangeHandler);
            Sys.UI.DomEvent.addHandler(fieldElement, 'blur', this._onBlurHandler);
        }
    },
    
    _getStringValue: function Sys_Mvc_FieldValidation$_getStringValue() {
        /// <returns type="String"></returns>
        return (this._fieldElements.length > 0) ? this._fieldElements[0].value : null;
    },
    
    _onErrorCountChanged: function Sys_Mvc_FieldValidation$_onErrorCountChanged() {
        if (!this._errors.length) {
            this._displaySuccess();
        }
        else {
            this._displayError();
        }
    },
    
    removeAllErrors: function Sys_Mvc_FieldValidation$removeAllErrors() {
        Array.clear(this._errors);
        this._onErrorCountChanged();
    },
    
    validate: function Sys_Mvc_FieldValidation$validate() {
        /// <returns type="Array" elementType="String"></returns>
        var allErrors = [];
        for (var i = 0; i < this._validators.length; i++) {
            var validator = this._validators[i];
            var thisErrors = validator.validate(this, this._fieldElements, this._getStringValue());
            if (thisErrors) {
                Array.addRange(allErrors, thisErrors);
            }
        }
        for (var i = 0; i < this._fieldElements.length; i++) {
            var fieldElement = this._fieldElements[i];
            fieldElement[Sys.Mvc.FieldValidation._hasValidationFiredTag] = true;
        }
        this.removeAllErrors();
        this.addErrors(allErrors);
        return allErrors;
    }
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.FormValidation

Sys.Mvc.FormValidation = function Sys_Mvc_FormValidation(formElement, validationSummaryElement) {
    /// <param name="formElement" type="Object" domElement="true">
    /// </param>
    /// <param name="validationSummaryElement" type="Object" domElement="true">
    /// </param>
    /// <field name="_validationSummaryErrorCss" type="String" static="true">
    /// </field>
    /// <field name="_validationSummaryValidCss" type="String" static="true">
    /// </field>
    /// <field name="_formValidationTag" type="String" static="true">
    /// </field>
    /// <field name="_onSubmitHandler" type="Sys.UI.DomEventHandler">
    /// </field>
    /// <field name="_errors" type="Array">
    /// </field>
    /// <field name="_fieldValidations" type="Array">
    /// </field>
    /// <field name="_formElement" type="Object" domElement="true">
    /// </field>
    /// <field name="_validationSummaryElement" type="Object" domElement="true">
    /// </field>
    /// <field name="_validationSummaryULElement" type="Object" domElement="true">
    /// </field>
    this._errors = [];
    this._fieldValidations = [];
    this._formElement = formElement;
    this._validationSummaryElement = validationSummaryElement;
    formElement[Sys.Mvc.FormValidation._formValidationTag] = this;
    if (validationSummaryElement) {
        var ulElements = validationSummaryElement.getElementsByTagName('ul');
        if (ulElements.length > 0) {
            this._validationSummaryULElement = ulElements[0];
        }
    }
    this._onSubmitHandler = Function.createDelegate(this, this._form_OnSubmit);
}
Sys.Mvc.FormValidation.enableClientValidation = function Sys_Mvc_FormValidation$enableClientValidation(options, userState) {
    /// <param name="options" type="Sys.Mvc.JsonValidationOptions">
    /// </param>
    /// <param name="userState" type="Object">
    /// </param>
    Sys.Application.add_load(Function.createDelegate(null, function(sender, e) {
        Sys.Mvc.FormValidation.parseJsonOptions(options);
    }));
}
Sys.Mvc.FormValidation._getFormElementsWithName = function Sys_Mvc_FormValidation$_getFormElementsWithName(formElement, name) {
    /// <param name="formElement" type="Object" domElement="true">
    /// </param>
    /// <param name="name" type="String">
    /// </param>
    /// <returns type="Array" elementType="Object" elementDomElement="true"></returns>
    var allElementsWithNameInForm = [];
    var allElementsWithName = document.getElementsByName(name);
    for (var i = 0; i < allElementsWithName.length; i++) {
        var thisElement = allElementsWithName[i];
        if (Sys.Mvc.FormValidation._isElementInHierarchy(formElement, thisElement)) {
            Array.add(allElementsWithNameInForm, thisElement);
        }
    }
    return allElementsWithNameInForm;
}
Sys.Mvc.FormValidation.getValidationForForm = function Sys_Mvc_FormValidation$getValidationForForm(formElement) {
    /// <param name="formElement" type="Object" domElement="true">
    /// </param>
    /// <returns type="Sys.Mvc.FormValidation"></returns>
    return formElement[Sys.Mvc.FormValidation._formValidationTag];
}
Sys.Mvc.FormValidation._isElementInHierarchy = function Sys_Mvc_FormValidation$_isElementInHierarchy(parent, child) {
    /// <param name="parent" type="Object" domElement="true">
    /// </param>
    /// <param name="child" type="Object" domElement="true">
    /// </param>
    /// <returns type="Boolean"></returns>
    while (child) {
        if (parent === child) {
            return true;
        }
        child = child.parentNode;
    }
    return false;
}
Sys.Mvc.FormValidation.parseJsonOptions = function Sys_Mvc_FormValidation$parseJsonOptions(options) {
    /// <param name="options" type="Sys.Mvc.JsonValidationOptions">
    /// </param>
    /// <returns type="Sys.Mvc.FormValidation"></returns>
    var formElement = $get(options.FormId);
    var validationSummaryElement = (!Sys.Mvc._validationUtil.stringIsNullOrEmpty(options.ValidationSummaryId)) ? $get(options.ValidationSummaryId) : null;
    var formValidation = new Sys.Mvc.FormValidation(formElement, validationSummaryElement);
    formValidation.enableDynamicValidation();
    for (var i = 0; i < options.Fields.length; i++) {
        var field = options.Fields[i];
        var fieldElements = Sys.Mvc.FormValidation._getFormElementsWithName(formElement, field.FieldName);
        var validationMessageElement = (!Sys.Mvc._validationUtil.stringIsNullOrEmpty(field.ValidationMessageId)) ? $get(field.ValidationMessageId) : null;
        var fieldValidation = new Sys.Mvc.FieldValidation(formValidation, fieldElements, validationMessageElement, field.ReplaceValidationMessageContents);
        for (var j = 0; j < field.ValidationRules.length; j++) {
            var rule = field.ValidationRules[j];
            var validator = Sys.Mvc.ValidatorRegistry.getValidator(rule);
            if (validator) {
                fieldValidation.addValidator(validator);
            }
        }
        fieldValidation.enableDynamicValidation();
        formValidation.addFieldValidation(fieldValidation);
    }
    return formValidation;
}
Sys.Mvc.FormValidation.prototype = {
    _onSubmitHandler: null,
    _formElement: null,
    _validationSummaryElement: null,
    _validationSummaryULElement: null,
    
    addError: function Sys_Mvc_FormValidation$addError(message) {
        /// <param name="message" type="String">
        /// </param>
        this.addErrors([ message ]);
    },
    
    addErrors: function Sys_Mvc_FormValidation$addErrors(messages) {
        /// <param name="messages" type="Array" elementType="String">
        /// </param>
        if (!Sys.Mvc._validationUtil.arrayIsNullOrEmpty(messages)) {
            Array.addRange(this._errors, messages);
            this._onErrorCountChanged();
        }
    },
    
    addFieldValidation: function Sys_Mvc_FormValidation$addFieldValidation(validation) {
        /// <param name="validation" type="Sys.Mvc.FieldValidation">
        /// </param>
        Array.add(this._fieldValidations, validation);
    },
    
    disableDynamicValidation: function Sys_Mvc_FormValidation$disableDynamicValidation() {
        Sys.UI.DomEvent.removeHandler(this._formElement, 'submit', this._onSubmitHandler);
    },
    
    _displayError: function Sys_Mvc_FormValidation$_displayError() {
        if (this._validationSummaryElement) {
            if (this._validationSummaryULElement) {
                Sys.Mvc._validationUtil.removeAllChildren(this._validationSummaryULElement);
                for (var i = 0; i < this._errors.length; i++) {
                    var liElement = document.createElement('li');
                    Sys.Mvc._validationUtil.setInnerText(liElement, this._errors[i]);
                    this._validationSummaryULElement.appendChild(liElement);
                }
            }
            Sys.UI.DomElement.removeCssClass(this._validationSummaryElement, Sys.Mvc.FormValidation._validationSummaryValidCss);
            Sys.UI.DomElement.addCssClass(this._validationSummaryElement, Sys.Mvc.FormValidation._validationSummaryErrorCss);
        }
    },
    
    _displaySuccess: function Sys_Mvc_FormValidation$_displaySuccess() {
        if (this._validationSummaryElement) {
            if (this._validationSummaryULElement) {
                this._validationSummaryULElement.innerHTML = '';
            }
            Sys.UI.DomElement.removeCssClass(this._validationSummaryElement, Sys.Mvc.FormValidation._validationSummaryErrorCss);
            Sys.UI.DomElement.addCssClass(this._validationSummaryElement, Sys.Mvc.FormValidation._validationSummaryValidCss);
        }
    },
    
    enableDynamicValidation: function Sys_Mvc_FormValidation$enableDynamicValidation() {
        Sys.UI.DomEvent.addHandler(this._formElement, 'submit', this._onSubmitHandler);
    },
    
    _form_OnSubmit: function Sys_Mvc_FormValidation$_form_OnSubmit(e) {
        /// <param name="e" type="Sys.UI.DomEvent">
        /// </param>
        var form = e.target;
        var errorMessages = this.validate(true);
        if (!Sys.Mvc._validationUtil.arrayIsNullOrEmpty(errorMessages)) {
            e.preventDefault();
        }
    },
    
    _onErrorCountChanged: function Sys_Mvc_FormValidation$_onErrorCountChanged() {
        if (!this._errors.length) {
            this._displaySuccess();
        }
        else {
            this._displayError();
        }
    },
    
    removeAllErrors: function Sys_Mvc_FormValidation$removeAllErrors() {
        Array.clear(this._errors);
        this._onErrorCountChanged();
    },
    
    validate: function Sys_Mvc_FormValidation$validate(replaceValidationSummary) {
        /// <param name="replaceValidationSummary" type="Boolean">
        /// </param>
        /// <returns type="Array" elementType="String"></returns>
        var allErrors = [];
        for (var i = 0; i < this._fieldValidations.length; i++) {
            var validation = this._fieldValidations[i];
            var thisErrors = validation.validate();
            if (thisErrors) {
                Array.addRange(allErrors, thisErrors);
            }
        }
        if (replaceValidationSummary) {
            this.removeAllErrors();
            this.addErrors(allErrors);
        }
        return allErrors;
    }
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.MvcHelpers

Sys.Mvc.MvcHelpers = function Sys_Mvc_MvcHelpers() {
}
Sys.Mvc.MvcHelpers._serializeSubmitButton = function Sys_Mvc_MvcHelpers$_serializeSubmitButton(element, offsetX, offsetY) {
    /// <param name="element" type="Object" domElement="true">
    /// </param>
    /// <param name="offsetX" type="Number" integer="true">
    /// </param>
    /// <param name="offsetY" type="Number" integer="true">
    /// </param>
    /// <returns type="String"></returns>
    if (element.disabled) {
        return null;
    }
    var name = element.name;
    if (name) {
        var tagName = element.tagName.toUpperCase();
        var encodedName = encodeURIComponent(name);
        var inputElement = element;
        if (tagName === 'INPUT') {
            var type = inputElement.type;
            if (type === 'submit') {
                return encodedName + '=' + encodeURIComponent(inputElement.value);
            }
            else if (type === 'image') {
                return encodedName + '.x=' + offsetX + '&' + encodedName + '.y=' + offsetY;
            }
        }
        else if ((tagName === 'BUTTON') && (name.length) && (inputElement.type === 'submit')) {
            return encodedName + '=' + encodeURIComponent(inputElement.value);
        }
    }
    return null;
}
Sys.Mvc.MvcHelpers._serializeForm = function Sys_Mvc_MvcHelpers$_serializeForm(form) {
    /// <param name="form" type="Object" domElement="true">
    /// </param>
    /// <returns type="String"></returns>
    var formElements = form.elements;
    var formBody = new Sys.StringBuilder();
    var count = formElements.length;
    for (var i = 0; i < count; i++) {
        var element = formElements[i];
        var name = element.name;
        if (!name || !name.length) {
            continue;
        }
        var tagName = element.tagName.toUpperCase();
        if (tagName === 'INPUT') {
            var inputElement = element;
            var type = inputElement.type;
            if ((type === 'text') || (type === 'password') || (type === 'hidden') || (((type === 'checkbox') || (type === 'radio')) && element.checked)) {
                formBody.append(encodeURIComponent(name));
                formBody.append('=');
                formBody.append(encodeURIComponent(inputElement.value));
                formBody.append('&');
            }
        }
        else if (tagName === 'SELECT') {
            var selectElement = element;
            var optionCount = selectElement.options.length;
            for (var j = 0; j < optionCount; j++) {
                var optionElement = selectElement.options[j];
                if (optionElement.selected) {
                    formBody.append(encodeURIComponent(name));
                    formBody.append('=');
                    formBody.append(encodeURIComponent(optionElement.value));
                    formBody.append('&');
                }
            }
        }
        else if (tagName === 'TEXTAREA') {
            formBody.append(encodeURIComponent(name));
            formBody.append('=');
            formBody.append(encodeURIComponent((element.value)));
            formBody.append('&');
        }
    }
    var additionalInput = form._additionalInput;
    if (additionalInput) {
        formBody.append(additionalInput);
        formBody.append('&');
    }
    return formBody.toString();
}
Sys.Mvc.MvcHelpers._asyncRequest = function Sys_Mvc_MvcHelpers$_asyncRequest(url, verb, body, triggerElement, ajaxOptions) {
    /// <param name="url" type="String">
    /// </param>
    /// <param name="verb" type="String">
    /// </param>
    /// <param name="body" type="String">
    /// </param>
    /// <param name="triggerElement" type="Object" domElement="true">
    /// </param>
    /// <param name="ajaxOptions" type="Sys.Mvc.AjaxOptions">
    /// </param>
    if (ajaxOptions.confirm) {
        if (!confirm(ajaxOptions.confirm)) {
            return;
        }
    }
    if (ajaxOptions.url) {
        url = ajaxOptions.url;
    }
    if (ajaxOptions.httpMethod) {
        verb = ajaxOptions.httpMethod;
    }
    if (body.length > 0 && !body.endsWith('&')) {
        body += '&';
    }
    body += 'X-Requested-With=XMLHttpRequest';
    var upperCaseVerb = verb.toUpperCase();
    var isGetOrPost = (upperCaseVerb === 'GET' || upperCaseVerb === 'POST');
    if (!isGetOrPost) {
        body += '&';
        body += 'X-HTTP-Method-Override=' + upperCaseVerb;
    }
    var requestBody = '';
    if (upperCaseVerb === 'GET' || upperCaseVerb === 'DELETE') {
        if (url.indexOf('?') > -1) {
            if (!url.endsWith('&')) {
                url += '&';
            }
            url += body;
        }
        else {
            url += '?';
            url += body;
        }
    }
    else {
        requestBody = body;
    }
    var request = new Sys.Net.WebRequest();
    request.set_url(url);
    if (isGetOrPost) {
        request.set_httpVerb(verb);
    }
    else {
        request.set_httpVerb('POST');
        request.get_headers()['X-HTTP-Method-Override'] = upperCaseVerb;
    }
    request.set_body(requestBody);
    if (verb.toUpperCase() === 'PUT') {
        request.get_headers()['Content-Type'] = 'application/x-www-form-urlencoded;';
    }
    request.get_headers()['X-Requested-With'] = 'XMLHttpRequest';
    var updateElement = null;
    if (ajaxOptions.updateTargetId) {
        updateElement = $get(ajaxOptions.updateTargetId);
    }
    var loadingElement = null;
    if (ajaxOptions.loadingElementId) {
        loadingElement = $get(ajaxOptions.loadingElementId);
    }
    var ajaxContext = new Sys.Mvc.AjaxContext(request, updateElement, loadingElement, ajaxOptions.insertionMode);
    var continueRequest = true;
    if (ajaxOptions.onBegin) {
        continueRequest = ajaxOptions.onBegin(ajaxContext) !== false;
    }
    if (loadingElement) {
        Sys.UI.DomElement.setVisible(ajaxContext.get_loadingElement(), true);
    }
    if (continueRequest) {
        request.add_completed(Function.createDelegate(null, function(executor) {
            Sys.Mvc.MvcHelpers._onComplete(request, ajaxOptions, ajaxContext);
        }));
        request.invoke();
    }
}
Sys.Mvc.MvcHelpers._onComplete = function Sys_Mvc_MvcHelpers$_onComplete(request, ajaxOptions, ajaxContext) {
    /// <param name="request" type="Sys.Net.WebRequest">
    /// </param>
    /// <param name="ajaxOptions" type="Sys.Mvc.AjaxOptions">
    /// </param>
    /// <param name="ajaxContext" type="Sys.Mvc.AjaxContext">
    /// </param>
    ajaxContext.set_response(request.get_executor());
    if (ajaxOptions.onComplete && ajaxOptions.onComplete(ajaxContext) === false) {
        return;
    }
    var statusCode = ajaxContext.get_response().get_statusCode();
    if ((statusCode >= 200 && statusCode < 300) || statusCode === 304 || statusCode === 1223) {
        if (statusCode !== 204 && statusCode !== 304 && statusCode !== 1223) {
            var contentType = ajaxContext.get_response().getResponseHeader('Content-Type');
            if ((contentType) && (contentType.indexOf('application/x-javascript') !== -1)) {
                eval(ajaxContext.get_data());
            }
            else {
                Sys.Mvc.MvcHelpers.updateDomElement(ajaxContext.get_updateTarget(), ajaxContext.get_insertionMode(), ajaxContext.get_data());
            }
        }
        if (ajaxOptions.onSuccess) {
            ajaxOptions.onSuccess(ajaxContext);
        }
    }
    else {
        if (ajaxOptions.onFailure) {
            ajaxOptions.onFailure(ajaxContext);
        }
    }
    if (ajaxContext.get_loadingElement()) {
        Sys.UI.DomElement.setVisible(ajaxContext.get_loadingElement(), false);
    }
}
Sys.Mvc.MvcHelpers.updateDomElement = function Sys_Mvc_MvcHelpers$updateDomElement(target, insertionMode, content) {
    /// <param name="target" type="Object" domElement="true">
    /// </param>
    /// <param name="insertionMode" type="Sys.Mvc.InsertionMode">
    /// </param>
    /// <param name="content" type="String">
    /// </param>
    if (target) {
        switch (insertionMode) {
            case Sys.Mvc.InsertionMode.replace:
                target.innerHTML = content;
                break;
            case Sys.Mvc.InsertionMode.insertBefore:
                if (content && content.length > 0) {
                    target.innerHTML = content + target.innerHTML.trimStart();
                }
                break;
            case Sys.Mvc.InsertionMode.insertAfter:
                if (content && content.length > 0) {
                    target.innerHTML = target.innerHTML.trimEnd() + content;
                }
                break;
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.AsyncForm

Sys.Mvc.AsyncForm = function Sys_Mvc_AsyncForm() {
}
Sys.Mvc.AsyncForm.handleClick = function Sys_Mvc_AsyncForm$handleClick(form, evt) {
    /// <param name="form" type="Object" domElement="true">
    /// </param>
    /// <param name="evt" type="Sys.UI.DomEvent">
    /// </param>
    var additionalInput = Sys.Mvc.MvcHelpers._serializeSubmitButton(evt.target, evt.offsetX, evt.offsetY);
    form._additionalInput = additionalInput;
}
Sys.Mvc.AsyncForm.handleSubmit = function Sys_Mvc_AsyncForm$handleSubmit(form, evt, ajaxOptions) {
    /// <param name="form" type="Object" domElement="true">
    /// </param>
    /// <param name="evt" type="Sys.UI.DomEvent">
    /// </param>
    /// <param name="ajaxOptions" type="Sys.Mvc.AjaxOptions">
    /// </param>
    evt.preventDefault();
    var body = Sys.Mvc.MvcHelpers._serializeForm(form);
    Sys.Mvc.MvcHelpers._asyncRequest(form.action, form.method || 'post', body, form, ajaxOptions);
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.RangeValidator

Sys.Mvc.RangeValidator = function Sys_Mvc_RangeValidator(errorMessage, minimum, maximum) {
    /// <param name="errorMessage" type="String">
    /// </param>
    /// <param name="minimum" type="Number">
    /// </param>
    /// <param name="maximum" type="Number">
    /// </param>
    /// <field name="_errorMessage$1" type="String">
    /// </field>
    /// <field name="_minimum$1" type="Number">
    /// </field>
    /// <field name="_maximum$1" type="Number">
    /// </field>
    Sys.Mvc.RangeValidator.initializeBase(this);
    this._errorMessage$1 = errorMessage;
    this._minimum$1 = minimum;
    this._maximum$1 = maximum;
}
Sys.Mvc.RangeValidator._create = function Sys_Mvc_RangeValidator$_create(rule) {
    /// <param name="rule" type="Sys.Mvc.JsonValidationRule">
    /// </param>
    /// <returns type="Sys.Mvc.RangeValidator"></returns>
    var min = rule.ValidationParameters['minimum'];
    var max = rule.ValidationParameters['maximum'];
    return new Sys.Mvc.RangeValidator(rule.ErrorMessage, min, max);
}
Sys.Mvc.RangeValidator.prototype = {
    _errorMessage$1: null,
    _minimum$1: null,
    _maximum$1: null,
    
    validate: function Sys_Mvc_RangeValidator$validate(validation, elements, value) {
        /// <param name="validation" type="Sys.Mvc.FieldValidation">
        /// </param>
        /// <param name="elements" type="Array" elementType="Object" elementDomElement="true">
        /// </param>
        /// <param name="value" type="String">
        /// </param>
        /// <returns type="Array" elementType="String"></returns>
        if (Sys.Mvc._validationUtil.stringIsNullOrEmpty(value)) {
            return null;
        }
        var n = Number.parseLocale(value);
        return (isNaN(n) || n < this._minimum$1 || n > this._maximum$1) ? [ this._errorMessage$1 ] : null;
    }
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.RegularExpressionValidator

Sys.Mvc.RegularExpressionValidator = function Sys_Mvc_RegularExpressionValidator(errorMessage, pattern) {
    /// <param name="errorMessage" type="String">
    /// </param>
    /// <param name="pattern" type="String">
    /// </param>
    /// <field name="_errorMessage$1" type="String">
    /// </field>
    /// <field name="_pattern$1" type="String">
    /// </field>
    Sys.Mvc.RegularExpressionValidator.initializeBase(this);
    this._errorMessage$1 = errorMessage;
    this._pattern$1 = pattern;
}
Sys.Mvc.RegularExpressionValidator._create = function Sys_Mvc_RegularExpressionValidator$_create(rule) {
    /// <param name="rule" type="Sys.Mvc.JsonValidationRule">
    /// </param>
    /// <returns type="Sys.Mvc.RegularExpressionValidator"></returns>
    var pattern = rule.ValidationParameters['pattern'];
    return new Sys.Mvc.RegularExpressionValidator(rule.ErrorMessage, pattern);
}
Sys.Mvc.RegularExpressionValidator.prototype = {
    _errorMessage$1: null,
    _pattern$1: null,
    
    validate: function Sys_Mvc_RegularExpressionValidator$validate(validation, elements, value) {
        /// <param name="validation" type="Sys.Mvc.FieldValidation">
        /// </param>
        /// <param name="elements" type="Array" elementType="Object" elementDomElement="true">
        /// </param>
        /// <param name="value" type="String">
        /// </param>
        /// <returns type="Array" elementType="String"></returns>
        if (Sys.Mvc._validationUtil.stringIsNullOrEmpty(value)) {
            return null;
        }
        var regExp = new RegExp(this._pattern$1);
        var matches = regExp.exec(value);
        return (!Sys.Mvc._validationUtil.arrayIsNullOrEmpty(matches) && matches[0].length === value.length) ? null : [ this._errorMessage$1 ];
    }
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.RequiredValidator

Sys.Mvc.RequiredValidator = function Sys_Mvc_RequiredValidator(errorMessage) {
    /// <param name="errorMessage" type="String">
    /// </param>
    /// <field name="_errorMessage$1" type="String">
    /// </field>
    Sys.Mvc.RequiredValidator.initializeBase(this);
    this._errorMessage$1 = errorMessage;
}
Sys.Mvc.RequiredValidator._create = function Sys_Mvc_RequiredValidator$_create(rule) {
    /// <param name="rule" type="Sys.Mvc.JsonValidationRule">
    /// </param>
    /// <returns type="Sys.Mvc.RequiredValidator"></returns>
    return new Sys.Mvc.RequiredValidator(rule.ErrorMessage);
}
Sys.Mvc.RequiredValidator._isRadioInputElement$1 = function Sys_Mvc_RequiredValidator$_isRadioInputElement$1(element) {
    /// <param name="element" type="Object" domElement="true">
    /// </param>
    /// <returns type="Boolean"></returns>
    if (element.tagName.toUpperCase() === 'INPUT') {
        var inputType = (element.type).toUpperCase();
        if (inputType === 'RADIO') {
            return true;
        }
    }
    return false;
}
Sys.Mvc.RequiredValidator._isSelectInputElement$1 = function Sys_Mvc_RequiredValidator$_isSelectInputElement$1(element) {
    /// <param name="element" type="Object" domElement="true">
    /// </param>
    /// <returns type="Boolean"></returns>
    if (element.tagName.toUpperCase() === 'SELECT') {
        return true;
    }
    return false;
}
Sys.Mvc.RequiredValidator._isTextualInputElement$1 = function Sys_Mvc_RequiredValidator$_isTextualInputElement$1(element) {
    /// <param name="element" type="Object" domElement="true">
    /// </param>
    /// <returns type="Boolean"></returns>
    if (element.tagName.toUpperCase() === 'INPUT') {
        var inputType = (element.type).toUpperCase();
        switch (inputType) {
            case 'TEXT':
            case 'PASSWORD':
            case 'FILE':
                return true;
        }
    }
    if (element.tagName.toUpperCase() === 'TEXTAREA') {
        return true;
    }
    return false;
}
Sys.Mvc.RequiredValidator.prototype = {
    _errorMessage$1: null,
    
    validate: function Sys_Mvc_RequiredValidator$validate(validation, elements, value) {
        /// <param name="validation" type="Sys.Mvc.FieldValidation">
        /// </param>
        /// <param name="elements" type="Array" elementType="Object" elementDomElement="true">
        /// </param>
        /// <param name="value" type="String">
        /// </param>
        /// <returns type="Array" elementType="String"></returns>
        if (!elements.length) {
            return null;
        }
        var sampleElement = elements[0];
        if (Sys.Mvc.RequiredValidator._isTextualInputElement$1(sampleElement)) {
            return this._validateTextualInput$1(sampleElement);
        }
        if (Sys.Mvc.RequiredValidator._isRadioInputElement$1(sampleElement)) {
            return this._validateRadioInput$1(elements);
        }
        if (Sys.Mvc.RequiredValidator._isSelectInputElement$1(sampleElement)) {
            return this._validateSelectInput$1((sampleElement).options);
        }
        return null;
    },
    
    _validateRadioInput$1: function Sys_Mvc_RequiredValidator$_validateRadioInput$1(elements) {
        /// <param name="elements" type="Array" elementType="Object" elementDomElement="true">
        /// </param>
        /// <returns type="Array" elementType="String"></returns>
        for (var i = 0; i < elements.length; i++) {
            var element = elements[i];
            if (element.checked) {
                return null;
            }
        }
        return [ this._errorMessage$1 ];
    },
    
    _validateSelectInput$1: function Sys_Mvc_RequiredValidator$_validateSelectInput$1(optionElements) {
        /// <param name="optionElements" type="DOMElementCollection">
        /// </param>
        /// <returns type="Array" elementType="String"></returns>
        for (var i = 0; i < optionElements.length; i++) {
            var element = optionElements[i];
            if (element.selected) {
                if (!Sys.Mvc._validationUtil.stringIsNullOrEmpty(element.value)) {
                    return null;
                }
            }
        }
        return [ this._errorMessage$1 ];
    },
    
    _validateTextualInput$1: function Sys_Mvc_RequiredValidator$_validateTextualInput$1(element) {
        /// <param name="element" type="Object" domElement="true">
        /// </param>
        /// <returns type="Array" elementType="String"></returns>
        return (Sys.Mvc._validationUtil.stringIsNullOrEmpty(element.value)) ? [ this._errorMessage$1 ] : null;
    }
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.StringLengthValidator

Sys.Mvc.StringLengthValidator = function Sys_Mvc_StringLengthValidator(errorMessage, minLength, maxLength) {
    /// <param name="errorMessage" type="String">
    /// </param>
    /// <param name="minLength" type="Number" integer="true">
    /// </param>
    /// <param name="maxLength" type="Number" integer="true">
    /// </param>
    /// <field name="_errorMessage$1" type="String">
    /// </field>
    /// <field name="_maxLength$1" type="Number" integer="true">
    /// </field>
    /// <field name="_minLength$1" type="Number" integer="true">
    /// </field>
    Sys.Mvc.StringLengthValidator.initializeBase(this);
    this._errorMessage$1 = errorMessage;
    this._minLength$1 = minLength;
    this._maxLength$1 = maxLength;
}
Sys.Mvc.StringLengthValidator._create = function Sys_Mvc_StringLengthValidator$_create(rule) {
    /// <param name="rule" type="Sys.Mvc.JsonValidationRule">
    /// </param>
    /// <returns type="Sys.Mvc.StringLengthValidator"></returns>
    var minLength = rule.ValidationParameters['minimumLength'];
    var maxLength = rule.ValidationParameters['maximumLength'];
    return new Sys.Mvc.StringLengthValidator(rule.ErrorMessage, minLength, maxLength);
}
Sys.Mvc.StringLengthValidator.prototype = {
    _errorMessage$1: null,
    _maxLength$1: 0,
    _minLength$1: 0,
    
    validate: function Sys_Mvc_StringLengthValidator$validate(validation, elements, value) {
        /// <param name="validation" type="Sys.Mvc.FieldValidation">
        /// </param>
        /// <param name="elements" type="Array" elementType="Object" elementDomElement="true">
        /// </param>
        /// <param name="value" type="String">
        /// </param>
        /// <returns type="Array" elementType="String"></returns>
        if (Sys.Mvc._validationUtil.stringIsNullOrEmpty(value)) {
            return null;
        }
        return (this._minLength$1 <= value.length && value.length <= this._maxLength$1) ? null : [ this._errorMessage$1 ];
    }
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc._validationUtil

Sys.Mvc._validationUtil = function Sys_Mvc__validationUtil() {
}
Sys.Mvc._validationUtil.arrayIsNullOrEmpty = function Sys_Mvc__validationUtil$arrayIsNullOrEmpty(array) {
    /// <param name="array" type="Array" elementType="Object">
    /// </param>
    /// <returns type="Boolean"></returns>
    return (!array || !array.length);
}
Sys.Mvc._validationUtil.stringIsNullOrEmpty = function Sys_Mvc__validationUtil$stringIsNullOrEmpty(value) {
    /// <param name="value" type="String">
    /// </param>
    /// <returns type="Boolean"></returns>
    return (!value || !value.length);
}
Sys.Mvc._validationUtil.elementSupportsEvent = function Sys_Mvc__validationUtil$elementSupportsEvent(element, eventAttributeName) {
    /// <param name="element" type="Object" domElement="true">
    /// </param>
    /// <param name="eventAttributeName" type="String">
    /// </param>
    /// <returns type="Boolean"></returns>
    return (eventAttributeName in element);
}
Sys.Mvc._validationUtil.removeAllChildren = function Sys_Mvc__validationUtil$removeAllChildren(element) {
    /// <param name="element" type="Object" domElement="true">
    /// </param>
    while (element.firstChild) {
        element.removeChild(element.firstChild);
    }
}
Sys.Mvc._validationUtil.setInnerText = function Sys_Mvc__validationUtil$setInnerText(element, innerText) {
    /// <param name="element" type="Object" domElement="true">
    /// </param>
    /// <param name="innerText" type="String">
    /// </param>
    var textNode = document.createTextNode(innerText);
    Sys.Mvc._validationUtil.removeAllChildren(element);
    element.appendChild(textNode);
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.Validator

Sys.Mvc.Validator = function Sys_Mvc_Validator() {
}


////////////////////////////////////////////////////////////////////////////////
// Sys.Mvc.ValidatorRegistry

Sys.Mvc.ValidatorRegistry = function Sys_Mvc_ValidatorRegistry() {
    /// <field name="_validators" type="Object" static="true">
    /// </field>
}
Sys.Mvc.ValidatorRegistry.get_creators = function Sys_Mvc_ValidatorRegistry$get_creators() {
    /// <value type="Object"></value>
    return Sys.Mvc.ValidatorRegistry._validators;
}
Sys.Mvc.ValidatorRegistry.getValidator = function Sys_Mvc_ValidatorRegistry$getValidator(rule) {
    /// <param name="rule" type="Sys.Mvc.JsonValidationRule">
    /// </param>
    /// <returns type="Sys.Mvc.Validator"></returns>
    var creator = Sys.Mvc.ValidatorRegistry._validators[rule.ValidationType];
    return (creator) ? creator(rule) : null;
}
Sys.Mvc.ValidatorRegistry._getDefaultValidators = function Sys_Mvc_ValidatorRegistry$_getDefaultValidators() {
    /// <returns type="Object"></returns>
    return { required: Function.createDelegate(null, Sys.Mvc.RequiredValidator._create), stringLength: Function.createDelegate(null, Sys.Mvc.StringLengthValidator._create), regularExpression: Function.createDelegate(null, Sys.Mvc.RegularExpressionValidator._create), range: Function.createDelegate(null, Sys.Mvc.RangeValidator._create) };
}


Sys.Mvc.AjaxContext.registerClass('Sys.Mvc.AjaxContext');
Sys.Mvc.AsyncHyperlink.registerClass('Sys.Mvc.AsyncHyperlink');
Sys.Mvc.FieldValidation.registerClass('Sys.Mvc.FieldValidation');
Sys.Mvc.FormValidation.registerClass('Sys.Mvc.FormValidation');
Sys.Mvc.MvcHelpers.registerClass('Sys.Mvc.MvcHelpers');
Sys.Mvc.AsyncForm.registerClass('Sys.Mvc.AsyncForm');
Sys.Mvc.Validator.registerClass('Sys.Mvc.Validator');
Sys.Mvc.RangeValidator.registerClass('Sys.Mvc.RangeValidator', Sys.Mvc.Validator);
Sys.Mvc.RegularExpressionValidator.registerClass('Sys.Mvc.RegularExpressionValidator', Sys.Mvc.Validator);
Sys.Mvc.RequiredValidator.registerClass('Sys.Mvc.RequiredValidator', Sys.Mvc.Validator);
Sys.Mvc.StringLengthValidator.registerClass('Sys.Mvc.StringLengthValidator', Sys.Mvc.Validator);
Sys.Mvc._validationUtil.registerClass('Sys.Mvc._validationUtil');
Sys.Mvc.ValidatorRegistry.registerClass('Sys.Mvc.ValidatorRegistry');
Sys.Mvc.FieldValidation._hasTextChangedTag = '__MVC_HasTextChanged';
Sys.Mvc.FieldValidation._hasValidationFiredTag = '__MVC_HasValidationFired';
Sys.Mvc.FieldValidation._inputElementErrorCss = 'input-validation-error';
Sys.Mvc.FieldValidation._inputElementValidCss = 'input-validation-valid';
Sys.Mvc.FieldValidation._validationMessageErrorCss = 'field-validation-error';
Sys.Mvc.FieldValidation._validationMessageValidCss = 'field-validation-valid';
Sys.Mvc.FormValidation._validationSummaryErrorCss = 'validation-summary-errors';
Sys.Mvc.FormValidation._validationSummaryValidCss = 'validation-summary-valid';
Sys.Mvc.FormValidation._formValidationTag = '__MVC_FormValidation';
Sys.Mvc.ValidatorRegistry._validators = Sys.Mvc.ValidatorRegistry._getDefaultValidators();

// ---- Do not remove this footer ----
// Generated using Script# v0.5.0.0 (http://projects.nikhilk.net)
// -----------------------------------
