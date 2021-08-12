app.controller('ApplicationNote', function($scope, $controller, $timeout, Utils, ngDialog) {
    Object.assign({
        name: 'ApplicationNote',
        displayName: 'Application Note'
      },
      $scope.section);

    angular.extend(this, $controller('Base', { $scope: $scope }));

    /**
     * Make AJAX request for the specified note.
     * @function loadNote
     * @param {any} id - The note id.
     * @returns {Promise}
     */
    function loadNote(id) {
      return $scope.load({
        url: '/Int/Application/Note/' + id,
        set: 'ngDialogData.note',
        overwrite: true
      }).then(function() {
        return $timeout(function() {
          $scope.ngDialogData.note.Content = $scope.ngDialogData.convertJson($scope.ngDialogData.note);
        });
      });
    }

    /**
     * Map attachment to $scope.ngDialogData.note
     * @function mapAttachmentToNote
     * @param {object} attachment - The attachment.
     * @returns {void}
     **/
    function mapAttachmentToNote(attachment) {
      $scope.ngDialogData.note.Attachment = attachment;
      $scope.ngDialogData.note.AttachmentDescription = attachment.Description;
      $scope.ngDialogData.note.AttachmentFileName = attachment.FileName;
      $scope.ngDialogData.note.AttachmentId = attachment.Id;
      $scope.ngDialogData.note.File = attachment.File;
    }

    /**
     * Initialize the form data.
     * @function init
     * @returns {Promise}
     **/
    function init() {
      if ($scope.ngDialogData.note.Id) {
        return loadNote($scope.ngDialogData.note.Id)
          .catch(angular.noop);
      }

      return angular.noop;
    }

    /**
     * Make AJAX request to delete the specified note.
     * @function deleteNote
     * @returns {Promise}
     **/
    $scope.deleteNote = function() {
      return $scope.confirmDialog('Delete Note', 'Do you want to delete this note?')
        .then(function() {
          return $scope.ajax({
            url: '/Int/Application/Note/Delete',
            method: 'PUT',
            data: function() {
              return $scope.ngDialogData.note;
            }
          }).then(function() {
            $scope.ngDialogData.note.Id = 0;
            $scope.confirm($scope.ngDialogData.note);
          });
        }).catch(angular.noop);
    }

    /**
     * Make AJAX request to save the specified note.
     * @function saveNote
     * @returns {Promise}
     **/
    $scope.saveNote = function() {
      return $scope.ajax({
        url: '/Int/Application/Note',
        method: $scope.ngDialogData.note.Id === 0 ? 'POST' : 'PUT',
        dataType: 'file',
        data: function() {
          var note = $scope.ngDialogData.note;
          note.DateAdded = $scope.toPST(note.DateAdded, 'YYYY-MM-DD h:mm:ss a');
          return note;
        }
      }).then(function(response) {
        $scope.ngDialogData.note = null;
        return $scope.confirm(response.data);
      }).catch(angular.noop);
    }

    /**
     * Open attachment dialog to choose a new attachment.
     * @function changeAttachment
     * @returns {Promise}
     **/
    $scope.changeAttachment = function() {
      return $scope.dzNoteAttachmentDialog('Change Attachment', $scope.ngDialogData.note.Attachment)
        .then(function(attachment) {
          mapAttachmentToNote(attachment);
        })
        .catch(angular.noop);
    }

    /**
     * Open attachment dialog to add a new attachment.
     * @function addAttachment
     * @returns {Promise}
     **/
    $scope.addAttachment = function() {
      return $scope.dzNoteAttachmentDialog('Add Attachment')
        .then(function(attachment) {
          mapAttachmentToNote(attachment);
        })
        .catch(angular.noop);
    }

    /**
     * Open confirmation dialog before removing file from note.
     * @function deleteAttachment
     * @returns {Promise}
     **/
    $scope.deleteAttachment = function() {
      return $scope.confirmDialog('Delete Attachment', 'Do you want to delete this attachment?')
        .then(function() {
          $scope.ngDialogData.note.AttachmentId = null;
          $scope.ngDialogData.note.Attachment = null;
          $scope.ngDialogData.note.File = null;
          $scope.ngDialogData.note.AttachmentDescription = null;
          $scope.ngDialogData.note.AttachmentFileName = null;
        })
        .catch(angular.noop);
    }

    /**
     * Open the modal file uploaded dialog for Notes.
     * @function attachmentDialog
     * @param {any} title - The title of the modal window.
     * @param {any} attachment - The attachment to update/add.
     * @returns {Promise}
     */
    $scope.dzNoteAttachmentDialog = function(title, attachment) {
      if (!title)
        title = 'Attachment';

      if (!attachment)
        attachment = Object.assign({ Id: 0 }, attachment);

      var permittedFileTypes = '';
      if ($scope.ngDialogData.permittedAttachmentTypes !== undefined)
        permittedFileTypes = Utils.replaceAll($scope.ngDialogData.permittedAttachmentTypes, /\|/g, ",");

      return ngDialog.openConfirm({
        template: '/content/dialogs/_NoteAttachments.html',
        data: {
          title: title,
          attachment: attachment,
          permittedAttachmentTypes: permittedFileTypes
        },
        controller: function($scope, Utils) {
          /**
           * Return the selected file in the promise.
           * @function save
           * @returns {Promise}
           **/
          $scope.save = function() {
            if ($scope.ngDialogData.attachment.FileName) {
              return $scope.confirm($scope.ngDialogData.attachment);
            } else {
              Utils.initValue($scope, 'errors.File', 'A file is required.');
            }
          };

          /**
           * Set the selected file as the active attachment.
           * @function fileChanged
           * @param {any} $files
           * @returns {void}
           */
          $scope.fileChanged = function($files) {
            if ($files.length) {
              $scope.ngDialogData.attachment.File = $files[0];
              $scope.ngDialogData.attachment.FileName = $scope.ngDialogData.attachment.File.name;
            }
          }

          $scope.dzOptions = {
            url: '/Int/Application/Note/DropzoneUpload',
            maxFilesize: '5', // Should be keyed to MaxUploadSizeInBytes
            autoProcessQueue: true,
            uploadMultiple: false,
            parallelUploads: 100,
            maxFiles: 1,
            paramName: "files",
            acceptedFiles: permittedFileTypes,
            addRemoveLinks: true
          };

          $scope.dzCallbacks = {
            'success': function(file, response) {
              $scope.ngDialogData.attachment.Id = response.AttachmentId;
              $scope.ngDialogData.attachment.FileName = response.FileName;
            },
            'removedfile': function(file) { // Called when 'remove file' is called when uploading a file
              $scope.ngDialogData.attachment.Id = 0;
              $scope.ngDialogData.attachment.FileName = '';
            }
          };
        }
      });
    }

    init();
  });

