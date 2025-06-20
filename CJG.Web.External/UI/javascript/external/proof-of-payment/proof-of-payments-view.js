app.controller('ProofOfPaymentsView', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'ProofOfPaymentsView',
    displayName: 'Proof of Payments',
    save: {
      url: function () {
        return '/Ext/Application/ProofOfPayment/Documents/' + $scope.section.grantApplicationId;
      },
      method: 'PUT',
      dataType: 'file',
      data: function () {
        var files = [];
        var attachments = $scope.section.attachments.filter(function (attachment) {
          if (typeof (attachment.File) !== 'undefined') {
            attachment.Index = files.length;
            files.push(attachment.File);
          }
          return attachment;
        });
        var model = {
          completeProofOfPayment: $scope.completeProofOfPayment === true, // We have to keep this separate from the model.IsComplete
          proofNotApplicable: $scope.model.ProofNotApplicable,  
          files: files,
          attachments: JSON.stringify(attachments)
        };

        return model;
      },
      backup: true
    },
    loaded: function () {
      return $scope.model && $scope.model.RowVersion && $scope.model.RowVersion === $scope.grantFile.RowVersion;
    },
    onSave: function () {
      $scope.section.attachments = [];
      window.location = $scope.section.redirectUrl;
    },
    onRefresh: function () {
      $scope.section.attachments = [];
      return loadAttachments().catch(angular.noop);
    },
    onCancel: function () {
      $scope.section.attachments = [];
    },
    grantApplicationId: $attrs.ngGrantApplicationId,
    completeProofOfPayment: null,

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
      url: '/Ext/Application/ProofOfPayment/Documents/' + $scope.section.grantApplicationId,
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

  /**
   * Mark the attachment for deletion.
   * @function removeAttachment
   * @param {any} index index
   * @returns {Promise} dialog
   */
  $scope.removeAttachment = function (index) {
    var attachment = $scope.model.Attachments[index];
    return $scope.confirmDialog('Remove Proof of Payment', 'Do you want to delete this Proof of Payment "' + attachment.FileName + '"?')
      .then(function (response) {
        if (response === true) {
          var attachment = $scope.model.Attachments.splice(index, 1)[0];
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
  $scope.addAttachment = function () {
    return $scope.attachmentDialog('Add Proof of Payment', {
      Id: 0,
      FileName: '',
      Description: '',
      File: {}
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
    return $scope.attachmentDialog('Update Proof of Payment', attachment)
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
