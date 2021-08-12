app.controller('SuccessStoryView', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'SuccessStoryView',
    displayName: 'Success Stories',
    save: {
      url: function () {
        return '/Ext/Application/SuccessStory/' + $scope.section.grantApplicationId;
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
          completeSuccessStory: $scope.completeSuccessStory === true, // We have to keep this separate from the model.IsComplete
          successfulOutcome: $scope.model.SuccessfulOutcome,
          noOutcomeReason: $scope.model.NoOutcomeReason,
          rowVersion: $scope.model.RowVersion,
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
      return loadSuccessStory().catch(angular.noop);
    },
    onCancel: function () {
      $scope.section.attachments = [];
    },
    grantApplicationId: $attrs.ngGrantApplicationId,
    completeSuccessStory: null,

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
  function loadSuccessStory() {
    return $scope.load({
      url: '/Ext/Application/SuccessStory/' + $scope.section.grantApplicationId,
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
      loadSuccessStory()
    ]).catch(angular.noop);
  }

  $scope.canSubmit = function () {
    var hasOutcome = $scope.model.SuccessfulOutcome != null;

    var meetsNoRequirement = $scope.model.SuccessfulOutcome === false
      && $scope.model.NoOutcomeReason != null
      && $scope.model.NoOutcomeReason.trim().length > 0;

    var meetsYesRequirement = $scope.model.SuccessfulOutcome === true
      && $scope.model.Attachments.length > 0;

    var isComplete = $scope.model.IsComplete;
    if (isComplete)
      return false;

    return hasOutcome && (meetsNoRequirement || meetsYesRequirement);
  }

  /**
   * Mark the attachment for deletion.
   * @function removeAttachment
   * @param {any} index index
   * @returns {Promise} dialog
   */
  $scope.removeAttachment = function (index) {
    var attachment = $scope.model.Attachments[index];
    return $scope.confirmDialog('Remove Success Story Attachment', 'Do you want to delete this Success Story Attachment "' + attachment.FileName + '"?')
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
  $scope.addAttachment = function (attachmentTypes) {
    return $scope.successStoryAttachmentDialog('Add Success Story Attachment', {
      Id: 0,
      FileName: '',
      Description: '',
      AttachmentType: 10,
      AttachmentTypes: attachmentTypes,
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
  $scope.changeAttachment = function (attachment, attachmentTypes) {
    attachment.AttachmentTypes = attachmentTypes;
    $scope.section.attachment = attachment;
    return $scope.successStoryAttachmentDialog('Update Success Story Attachment', attachment)
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
