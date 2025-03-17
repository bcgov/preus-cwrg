app.controller('ApplicationAttachmentsView', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'ApplicationAttachmentsView',
    displayName: 'Application Attachments',
    save: {
      url: function() {
        return '/Ext/Application/Attachments/' + $scope.section.grantApplicationId;
      },
      method: 'PUT',
      dataType: 'file',
      data: function() {
        var files = [];
        var attachments = $scope.section.attachments.filter(function(attachment) {
          if (typeof (attachment.File) !== 'undefined') {
            attachment.Index = files.length;
            files.push(attachment.File);
          }
          return attachment;
        });

        var model = {
          files: files,
          attachments: JSON.stringify(attachments),
          notRequestingESS: $scope.model.NotRequestingESS
      };

        return model;
      },
      backup: true
    },
    loaded: function() {
      return $scope.model && $scope.model.RowVersion && $scope.model.RowVersion === $scope.grantFile.RowVersion;
    },
    onSave: function() {
      $scope.section.attachments = [];
      window.location = $scope.section.redirectUrl;
    },
    onRefresh: function() {
      $scope.section.attachments = [];
      return loadAttachments().catch(angular.noop);
    },
    onCancel: function() {
      $scope.section.attachments = [];
    },
    grantApplicationId: $attrs.ngGrantApplicationId,
    redirectUrl: $attrs.ngRedirectUrl,
    attachments: []
  };

  $scope.grantFile = {
    Id: $attrs.grantApplicationId
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request to fetch attachment data
   * @function loadAttachments
   * @returns {Promise}
   **/
  function loadAttachments() {
    return $scope.load({
      url: '/Ext/Application/Attachments/' + $scope.section.grantApplicationId,
      set: 'model'
    });
  }

  /**
   * Initialize form data.
   * @function init
   * @returns {Promise}
   **/
  function init() {
    return Promise.all([
      loadAttachments()
    ]).catch(angular.noop);
  }

  $scope.canProceed = function () {
    if ($scope.model.Attachments == undefined)
      return false;

    if ($scope.model.Attachments.length == 0)
      return false;

    const projectDescriptions = $scope.model.Attachments.filter((d) => d.DocumentType === 1).length;
    const employerSupportForms = $scope.model.Attachments.filter((d) => d.DocumentType === 2).length;
    const skillTrainingDocuments = $scope.model.Attachments.filter((d) => d.DocumentType === 3).length;
    const essDocuments = $scope.model.Attachments.filter((d) => d.DocumentType === 4).length;
    const requireEss = !$scope.model.NotRequestingESS;

    let metEssRequirement = requireEss ? essDocuments >= 1 : true;

    return projectDescriptions >= 1 && employerSupportForms >= 1 && skillTrainingDocuments >= 1 && metEssRequirement;
  }

  /**
   * Mark the attachment for deletion.
   * @function removeAttachment
   * @param {any} index index
   * @returns {Promise} dialog
   */
  $scope.removeAttachment = function (index, documentType = 0) {
    var attachmentDocument = $scope.model.Attachments.filter((d) => d.DocumentType === documentType)[index];
    var attachmentIndex = $scope.model.Attachments.indexOf(attachmentDocument);

    return $scope.confirmDialog('Remove Attachment', 'Do you want to delete this attachment "' + attachmentDocument.FileName + '"?')
      .then(function (response) {
        if (response === true) {
          var attachment = $scope.model.Attachments.splice(attachmentIndex, 1)[0];
          attachment.Delete = true;
          var i = $scope.section.attachments.indexOf(attachment);

          if (i === -1) {
            $scope.section.attachments.push(attachment);
          } else if (attachment.Id === 0) {
            $scope.section.attachments.splice(i, 1);
          }
        }
      }).catch(angular.noop);
  };

  /**
   * Open modal file uploader popup and then add the new file to the model.
   * @function addAttachment
   * @returns {void}
   **/
  $scope.addAttachment = function (documentType = 0) {
    return $scope.attachmentDialog('Add Attachment', {
      Id: 0,
      FileName: '',
      Description: '',
      File: {},
      AttachmentType: 0,
      DocumentType: documentType,
      ReferenceId: Math.floor(Math.random() * 99999999999)
    })
      .then(function (attachment) {
        $scope.model.Attachments.push(attachment);
        $scope.section.attachments.push(attachment);
      })
      .catch(angular.noop);
  };

  /**
   * Open modal file uploader popup and allow user to update the attachment and/or file.
   * @function changeAttachment
   * @param {any} attachment - The attachment to update.
   * @returns {void}
   */
  $scope.changeAttachment = function (attachment) {
    $scope.section.attachment = attachment;
    return $scope.attachmentDialog('Update Attachment', attachment)
      .then(function (attachment) {
        if ($scope.section.attachments.indexOf(attachment) === -1) {
          $scope.section.attachments.push(attachment);
        }
      })
      .catch(angular.noop);
  };

  /**
   * Cancel the changes to attachments and redirect to specified URL.
   * @function cancel
   * @returns {void}
   **/
  $scope.cancel = function () {
    window.location = $scope.section.redirectUrl;
  }

  init();
});
