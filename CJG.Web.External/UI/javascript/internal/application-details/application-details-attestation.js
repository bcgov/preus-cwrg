app.controller('Attestation', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'Attestation',
    displayName: 'Attestation',
    save: {
      url: '/Int/Application/Attestation/',
      method: 'PUT',
      dataType: 'file',
      data: function () {
        var model = {
          grantApplicationId: $scope.model.Id,
          completeAttestation: $scope.model.CompleteAttestation,
          attestationNotApplicable: $scope.model.AttestationNotApplicable,
          allocatedCosts: $scope.model.AllocatedCosts,
          files: $scope.section.documents.filter(function (attachment) {
            return !attachment.Delete && typeof (attachment.File) !== 'undefined';
          }).map(function (attachment, index) {
            attachment.Index = index; // Map object to file array.
            var file = attachment.File; // Add file to array.
            return file;
          }),
          documents: JSON.stringify($scope.section.documents)
        };

        return model;
      },
      backup: true
    },
    onShow: function () {
      return angular.noop;
    },
    loaded: function () {
      return $scope.model && $scope.model.RowVersion && $scope.model.RowVersion === $scope.grantFile.RowVersion;
    },
    onSave: function () {
      $scope.section.documents = [];
      $scope.emit('refresh', { target: 'ApplicationNotes', force: true });
    },
    onRefresh: function () {
      $scope.section.documents = [];
      return loadAttestationDetails().catch(angular.noop);
    },
    onCancel: function () {
      $scope.section.documents = [];
    },
    documents: []
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request to fetch attestation data
   * @function loadAttachments
   * @returns {Promise}
   **/
  function loadAttestationDetails() {
    return $scope.load({
      url: '/Int/Application/Attestation/' + $scope.parent.grantApplicationId,
      set: 'model'
    });
  }

  /**
   * Initialize section data.
   * @function init
   * @returns {Promise}
   **/
  $scope.init = function () {
    return Promise.all([
      loadAttestationDetails()
    ]).catch(angular.noop);
  }

  $scope.toggleAttestation = function () {
    return $scope.ajax({
      url: '/Int/Application/Attestation/Toggle/' + $scope.parent.grantApplicationId + '?rowVersion=' + encodeURIComponent($scope.grantFile.RowVersion),
      method: 'PUT'
    }).then(function (response) {
      $scope.emit('update', {
        grantFile: {
          AttestationState: response.data.AttestationState,
          AttestationStateDescription: response.data.AttestationStateDescription,
          RowVersion: response.data.RowVersion
        }
      });
      $scope.emit('refresh', { target: 'ApplicationNotes', force: true });
    }).catch(angular.noop);
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

  /**
   * Open modal file uploader popup and then add the new file to the model.
   * @function addAttachment
   * @returns {void}
   **/
  $scope.addDocument = function () {
    return $scope.attachmentDialog('Add Participant Financial Supports Tracker', {
        Id: 0,
        FileName: '',
        Description: '',
        File: {}
      })
      .then(function (attachment) {
        $scope.model.Documents.push(attachment);
        $scope.section.documents.push(attachment);
      })
      .catch(angular.noop);
  }

  /**
   * Open modal file uploader popup and allow user to update the attachment and/or file.
   * @function changeAttachment
   * @param {any} attachment - The attachment to update.
   * @returns {void}
   */
  $scope.changeDocument = function (attachment) {
    $scope.section.document = attachment;
    return $scope.attachmentDialog('Update Participant Financial Supports Tracker', attachment)
      .then(function (attachment) {
        $scope.section.documents.push(attachment); // TODO: Fix
      })
      .catch(angular.noop);
  }

  /**
   * Mark the attachment for deletion.
   * @function removeAttachment
   * @param {any} attachment
   * @returns {Promise}
   */
  $scope.removeDocument = function (index) {
    var attachment = $scope.model.Documents[index];
    return $scope.confirmDialog('Remove Participant Financial Supports Tracker', 'Do you want to delete this Participant Financial Supports Tracker "' + attachment.FileName + '"?')
      .then(function (response) {
        if (response === true) {
          var attachment = $scope.model.Documents.splice(index, 1)[0];
          attachment.Delete = true;
          $scope.section.documents.push(attachment);
        }
      }).catch(angular.noop);
  }
});
