app.controller('AttestationView', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'AttestationView',
    displayName: 'Attestation',
    save: {
      url: function () {
        return '/Ext/Application/Attestation/' + $scope.section.grantApplicationId;
      },
      method: 'PUT',
      dataType: 'file',
      //data: function () {
      //  return $scope.model;
      //},
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
          completeAttestation: $scope.model.CompleteAttestation,
          attestationNotApplicable: $scope.model.AttestationNotApplicable,
          allocatedCosts: $scope.model.AllocatedCosts,
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
      return loadAttestation().catch(angular.noop);
    },
    onCancel: function () {
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
   * Make AJAX request to fetch Attestation data
   * @function loadAttachments
   * @returns {Promise}
   **/
  function loadAttestation() {
    return $scope.load({
      url: '/Ext/Application/Attestation/' + $scope.section.grantApplicationId,
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
      loadAttestation()
    ]).catch(angular.noop);
  }

  $scope.recalculateAttestation = function () {
    let allocatedCosts = $scope.model.AllocatedCosts;
    if (isNaN(allocatedCosts) || allocatedCosts < 0)
      allocatedCosts = 0;

    let newFunds = $scope.model.ClaimedCosts - allocatedCosts;
    if (newFunds < 0)
      newFunds = 0;

    $scope.model.UnusedFunds = newFunds;
  };

  $scope.cancel = function () {
    window.location = $scope.section.redirectUrl;
  }

  /**
   * Mark the attachment for deletion.
   * @function removeAttachment
   * @param {any} index index
   * @returns {Promise} dialog
   */
  $scope.removeAttachment = function (index) {
    var attachment = $scope.model.Attachments[index];
    return $scope.confirmDialog('Remove Participant Financial Supports Tracker', 'Do you want to delete this Participant Financial Supports Tracker "' + attachment.FileName + '"?')
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
    return $scope.attachmentDialog('Add Participant Financial Supports Tracker', {
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
    return $scope.attachmentDialog('Update Participant Financial Supports Tracker', attachment)
      .then(function (attachment) {
        if ($scope.section.attachments.indexOf(attachment) === -1) {
          $scope.section.attachments.push(attachment);
        }
      })
      .catch(angular.noop);
  };

  init();
});
