app.controller('EvaluationAttachments', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'EvaluationAttachments',
    displayName: 'Evaluation Resources',
    save: {
      url: '/Int/Admin/EvaluationForm/Attachments',
      method: 'PUT',
      dataType: 'file',
      data: function () {
        var model = {
          grantApplicationId: $scope.model.Id,
          files: $scope.section.attachments.filter(function (attachment) {
            return !attachment.Delete && typeof (attachment.File) !== 'undefined';
          }).map(function (attachment, index) {
            attachment.Index = index; // Map object to file array.
            var file = attachment.File; // Add file to array.
            return file;
          }),
          attachments: JSON.stringify($scope.section.attachments)
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
    },
    onRefresh: function () {
      $scope.section.attachments = [];
      return loadAttachments().catch(angular.noop);
    },
    onCancel: function () {
      $scope.section.attachments = [];
    },
    attachments: []
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request to fetch attachment data
   * @function loadAttachments
   * @returns {Promise}
   **/
  function loadAttachments() {
    return $scope.load({
      url: '/Int/Admin/EvaluationForm/Attachments',
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

  /**
   * Mark the attachment for deletion.
   * @function removeAttachment
   * @param {any} attachment
   * @returns {Promise}
   */
  $scope.removeAttachment = function (index) {
    var attachment = $scope.model.Attachments[index];
    return $scope.confirmDialog('Remove Resource', 'Do you want to delete this resource "' + attachment.FileName + '"?')
      .then(function (response) {
        if (response === true) {
          var attachment = $scope.model.Attachments.splice(index, 1)[0];
          attachment.Delete = true;
          $scope.section.attachments.push(attachment);
        }
      }).catch(angular.noop);
  }

  /**
   * Open modal file uploader popup and then add the new file to the model.
   * @function addAttachment
   * @returns {void}
   **/
  $scope.addAttachment = function () {
    return $scope.evaluationAttachmentDialog('Add Resource', {
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
  }

  /**
   * Open modal file uploader popup and allow user to update the attachment and/or file.
   * @function changeAttachment
   * @param {any} attachment - The attachment to update.
   * @returns {void}
   */
  $scope.changeAttachment = function (attachment) {
    $scope.section.attachment = attachment;
    return $scope.evaluationAttachmentDialog('Update Resource', attachment)
      .then(function (attachment) {
        $scope.section.attachments.push(attachment); // TODO: Fix
      })
      .catch(angular.noop);
  }
});
