app.controller('ProofOfPayment', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'ProofOfPayment',
    displayName: 'Proof of Payment',
    save: {
      url: '/Int/Application/ProofOfPayment/Documents',
      method: 'PUT',
      dataType: 'file',
      data: function () {
        var model = {
          grantApplicationId: $scope.model.Id,
          proofNotApplicable: $scope.model.ProofNotApplicable,
          files: $scope.section.documents.filter(function(attachment) {
            return !attachment.Delete && typeof (attachment.File) !== 'undefined';
          }).map(function(attachment, index) {
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
      return loadAttachments().catch(angular.noop);
    },
    onCancel: function () {
      $scope.section.documents = [];
    },
    documents: []
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request to fetch attachment data
   * @function loadAttachments
   * @returns {Promise}
   **/
  function loadAttachments() {
    return $scope.load({
      url: '/Int/Application/ProofOfPayment/Documents/' + $scope.parent.grantApplicationId,
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
      loadAttachments()
    ]).catch(angular.noop);
  }

  $scope.toggleProofOfPayments = function () {
    return $scope.ajax({
      url: '/Int/Application/ProofOfPayment/Toggle/' + $scope.parent.grantApplicationId + '?rowVersion=' + encodeURIComponent($scope.grantFile.RowVersion),
      method: 'PUT'
    }).then(function (response) {
      $scope.emit('update', {
        grantFile: {
          ProofOfPaymentState: response.data.ProofOfPaymentState,
          ProofOfPaymentStateDescription: response.data.ProofOfPaymentStateDescription,
          RowVersion: response.data.RowVersion
        }
      });
      $scope.emit('refresh', { target: 'ApplicationNotes', force: true });
    }).catch(angular.noop);
  }
  
  /**
   * Open modal file uploader popup and then add the new file to the model.
   * @function addAttachment
   * @returns {void}
   **/
  $scope.addDocument = function () {
    return $scope.attachmentDialog('Add Proof of Payment', {
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
    return $scope.attachmentDialog('Update Proof of Payment', attachment)
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
    return $scope.confirmDialog('Remove Proof of Payment', 'Do you want to delete this Proof of Payment "' + attachment.FileName + '"?')
      .then(function (response) {
        if (response === true) {
          var attachment = $scope.model.Documents.splice(index, 1)[0];
          attachment.Delete = true;
          $scope.section.documents.push(attachment);
        }
      }).catch(angular.noop);
  }
});
