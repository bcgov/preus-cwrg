app.controller('TrainingProviderHistory', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'TrainingProviderHistory',
    onRefresh: function () {
      return loadTrainingProviderHistory().catch(angular.noop);
    },
    attachments: [],
    documentsUpdated: false
  };

  angular.extend(this, $controller('Section', { $scope, $attrs }));

  /**
   * Make AJAX request to load training provider history data.
   * @function loadTrainingProviderHistory
   * @returns {Promise}
   **/
  function loadTrainingProviderHistory() {
    return $scope.load({
      url: '/Int/Training/Provider/History/' + $attrs.trainingProviderId,
      set: 'model'
    })
      .then(function () {
        return $timeout(function () {
          $scope.originalNotes = $scope.model.TrainingProviderNotes;
        });
      })
      .catch(angular.noop);
  }

  /**
* Make AJAX request to fetch attachment data
* @function loadDocuments
* @returns {Promise}
**/
  function loadDocuments() {
    return $scope.load({
      url: '/Int/Training/Provider/History/Documents/' + $attrs.trainingProviderId,
      set: 'documents'
    });
  }

  /**
   * Fetch all the data for the form.
   * @function init
   * @returns {Promise}
   **/
  function init() {
    return Promise.all([
      loadTrainingProviderHistory(),
      loadDocuments()
    ]);
  }

  /**
   * Make AJAX request to update the note.
   * @function updateNote
   * @returns {Promise} promise
   **/
  $scope.updateNote = function () {
    return $scope.ajax({
      url: '/Int/Training/Provider/History/Note/' + $scope.model.TrainingProviderInventoryId,
      method: 'PUT',
      data: {
        id: $scope.model.TrainingProviderInventoryId,
        notesText: $scope.model.TrainingProviderNotes,
        rowVersion: $scope.model.RowVersion
      }
    })
      .then(function (response) {
        return $timeout(function () {
          $scope.setAlert({ response: { status: 200 }, message: 'Training Provider notes have been saved successfully' });
          $scope.model.RowVersion = response.data.RowVersion;
          $scope.originalNotes = $scope.model.TrainingProviderNotes;
        });
      })
      .catch(angular.noop);
  };

  /**
   * Check if notes are different
   * @function checkNotesDiff
   * @returns {boolean}
   **/
  $scope.checkNotesDiff = function () {
    return $scope.originalNotes === $scope.model.TrainingProviderNotes;
  };

  /**
   * Reset the notes
   * @function resetNote
   * @returns {void}
   **/
  $scope.resetNote = function () {
    return $scope.model.TrainingProviderNotes = $scope.originalNotes;
  };

  /**
   * Delete the training provider.
   * @function deleteProvider
   * @returns {Promise} promise
   **/
  $scope.deleteProvider = function () {
    return $scope.confirmDialog('Delete', 'Are you sure you want to remove this training provider from the inventory?')
      .then(function () {
        return $scope.ajax({
          url: '/Int/Training/Provider/Inventory/Delete/',
          method: 'PUT',
          data: {
            id: $scope.model.TrainingProviderInventoryId,
            rowVersion: $scope.model.RowVersion
          }
        })
          .then(function () {
            window.location = '/Int/Training/Provider/Inventory/View';
          })
          .catch(angular.noop);
      })
      .catch(angular.noop);
  };

 /**
 * Make AJAX request to update the note.
 * @function updateNote
 * @returns {Promise} promise
 **/
  $scope.updateDocuments = function () {
    return $scope.ajax({
        url: '/Int/Training/Provider/History/Documents/',
        method: 'PUT',
        dataType: 'file',
        data: function () {
          var model = {
            trainingProviderInventoryId: $scope.model.TrainingProviderInventoryId,
            files: $scope.section.attachments.filter(function (attachment) {
              return !attachment.Delete && typeof (attachment.File) !== 'undefined';
            }).map(function (attachment, index) {
              attachment.Index = index; // Map object to file array.
              var file = attachment.File; // Add file to array.
              return file;
            }),
            documents: JSON.stringify($scope.section.attachments)
          };

          return model;
        }
      })
      .then(function (response) {
        return $timeout(function () {
          $scope.setAlert({ response: { status: 200 }, message: 'Documents have been saved successfully' });
          $scope.section.attachments = [];
          $scope.section.documentsUpdated = false;
          loadDocuments();
        });
      })
      .catch(angular.noop);
  };

 /**
 * Open modal file uploader popup and then add the new file to the model.
 * @function addAttachment
 * @returns {void}
 **/
  $scope.addDocument = function () {
    return $scope.historyAttachmentDialog('Add Document', {
        Id: 0,
        FileName: '',
        Description: '',
        File: {}
      })
      .then(function (attachment) {
        $scope.documents.Documents.push(attachment);
        $scope.section.attachments.push(attachment);
        $scope.section.documentsUpdated = true;
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
    var attachment = $scope.documents.Documents[index];
    return $scope.confirmDialog('Remove Document', 'Do you want to delete the document "' + attachment.FileName + '"?')
      .then(function (response) {
        if (response === true) {
          var attachment = $scope.documents.Documents.splice(index, 1)[0];
          attachment.Delete = true;
          $scope.section.attachments.push(attachment);
          $scope.section.documentsUpdated = true;
        }
      }).catch(angular.noop);
  }

  /**
   * Open modal file uploader popup and allow user to update the attachment and/or file.
   * @function changeAttachment
   * @param {any} attachment - The attachment to update.
   * @returns {void}
   */
  $scope.changeDocument = function (attachment) {
    $scope.section.attachment = attachment;
    return $scope.historyAttachmentDialog('Update Document', attachment)
      .then(function (attachment) {
        $scope.section.attachments.push(attachment); // TODO: Fix
        $scope.section.documentsUpdated = true;
      })
      .catch(angular.noop);
  }

  /**
   * Get the filtered training provider history.
   * @function getTrainingProviderHistory
   * @param {string} pageKeyword - The search filter keyword.
   * @param {int} page - The page number.
   * @param {int} quantity - The number of items in a page.
   * @returns {Promise}
   **/
  $scope.getTrainingProviderHistory = function (pageKeyword, page, quantity) {
    return $scope.ajax({
      url: '/Int/Training/Provider/History/Search/' + $scope.model.TrainingProviderInventoryId + '/' + page + '/' + quantity + (pageKeyword ? '?search=' + pageKeyword : '')
    })
      .then(function (response) {
        return Promise.resolve(response.data);
      })
      .catch(angular.noop);
  };

  init();
});
