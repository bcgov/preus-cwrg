app.controller('SuccessStories', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'SuccessStories',
    displayName: 'Success Stories',
    save: {
      url: '/Int/Application/SuccessStories',
      method: 'PUT',
      dataType: 'file',
      data: function () {
        var model = {
          grantApplicationId: $scope.model.Id,
          successfulOutcome: $scope.model.SuccessfulOutcome,
          noOutcomeReason: $scope.model.NoOutcomeReason,
          rowVersion: $scope.model.RowVersion,
          files: $scope.section.documents.filter(function(attachment) {
            return !attachment.Delete && typeof (attachment.File) !== 'undefined';
          }).map(function(attachment, index) {
            attachment.Index = index; // Map object to file array.
            var file = attachment.File; // Add file to array.
            return file;
          }),
          attachments: JSON.stringify($scope.section.documents)
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
      return loadSuccessStory()
        .catch(angular.noop);
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
  function loadSuccessStory() {
    return $scope.load({
      url: '/Int/Application/SuccessStories/Documents/' + $scope.parent.grantApplicationId,
      set: 'model'
    });
  }

  $scope.init = function () {
    return Promise.all([
      loadSuccessStory()
    ]).catch(angular.noop);
  }

  $scope.toggleSuccessStory = function () {
    return $scope.ajax({
      url: '/Int/Application/SuccessStories/Toggle/' + $scope.parent.grantApplicationId + '?rowVersion=' + encodeURIComponent($scope.grantFile.RowVersion),
      method: 'PUT'
    }).then(function (response) {
      $scope.emit('update',
        {
          grantFile: {
            SuccessStoriesState: response.data.SuccessStoriesState,
            SuccessStoriesStateDescription: response.data.SuccessStoriesStateDescription,
            RowVersion: response.data.RowVersion
          }
        });
      $scope.emit('refresh', { target: 'ApplicationNotes', force: true });
    }).catch(angular.noop);
  }
  
  /**
   * Open modal file uploader popup and then add the new file to the model.
   * @function addDocument
   * @returns {void}
   **/
  $scope.addDocument = function (attachmentTypes) {
    return $scope.successStoryAttachmentDialog('Add Success Story Attachment', {
        Id: 0,
        FileName: '',
        Description: '',
        File: {},
        AttachmentType: 10,
        AttachmentTypes: attachmentTypes
      })
      .then(function (attachment) {
        $scope.model.Documents.push(attachment);
        $scope.section.documents.push(attachment);
      })
      .catch(angular.noop);
  }

  /**
   * Open modal file uploader popup and allow user to update the attachment and/or file.
   * @function changeDocument
   * @param {any} attachment - The attachment to update.
   * @returns {void}
   */
  $scope.changeDocument = function (attachment, attachmentTypes) {
    attachment.AttachmentTypes = attachmentTypes;
    $scope.section.document = attachment;

    return $scope.successStoryAttachmentDialog('Update Success Story Attachment', attachment)
      .then(function (attachment) {
        $scope.section.documents.push(attachment); // TODO: Fix
      })
      .catch(angular.noop);
  }

  /**
   * Mark the attachment for deletion.
   * @function removeDocument
   * @param {any} attachment
   * @returns {Promise}
   */
  $scope.removeDocument = function (index) {
    var attachment = $scope.model.Documents[index];
    return $scope.confirmDialog('Remove Success Story Attachment', 'Do you want to delete this Success Story Attachment "' + attachment.FileName + '"?')
      .then(function (response) {
        if (response === true) {
          var attachment = $scope.model.Documents.splice(index, 1)[0];
          attachment.Delete = true;
          $scope.section.documents.push(attachment);
        }
      }).catch(angular.noop);
  }
});
