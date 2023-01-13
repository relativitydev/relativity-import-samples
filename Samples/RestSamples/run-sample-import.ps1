#import
$global:rootDir = "$PSScriptRoot"
. "$global:rootDir\Helpers\AuthClass.ps1"
. "$global:rootDir\Helpers\WebRequestClass.ps1"

$hostAddress = "https://sample-host/"
$userName = "sample@username"
$password = "password!"

$global:Auth = [Auth]::new($hostAddress, $userName, $password)
$global:WebRequest = [WebRequest]::new($global:Auth)

# Uncomment the samples you wish to run:
# NOTE: Copy sample source files into destination location.

Describe "Sample import" {
    . "$global:rootDir\SamplesCollection\sample01-import-native-files.ps1"

    # . "$global:rootDir\SamplesCollection\sample02-import-documents-in-overlay-mode.ps1"

    # . "$global:rootDir\SamplesCollection\sample03-import-from-two-data-sources.ps1"

    # . "$global:rootDir\SamplesCollection\sample04-add-data-source-to-running-job.ps1"

    # . "$global:rootDir\SamplesCollection\sample05-import-documents-with-extracted-text.ps1"

    # . "$global:rootDir\SamplesCollection\sample06-import-documents-to-selected-folder.ps1"

    # . "$global:rootDir\SamplesCollection\sample07-direct-import-settings-for-documents.ps1"

    # . "$global:rootDir\SamplesCollection\sample08-import-images.ps1"

    # . "$global:rootDir\SamplesCollection\sample09-import-production-files.ps1"

    # . "$global:rootDir\SamplesCollection\sample10-import-images-in-append-overlay-mode.ps1"

    # . "$global:rootDir\SamplesCollection\sample11-direct-import-settings-for-images.ps1"

    # . "$global:rootDir\SamplesCollection\sample12-import-relativity-dynamic-object.ps1"

    # . "$global:rootDir\SamplesCollection\sample13-import-rdo-with-parent.ps1"

    # . "$global:rootDir\SamplesCollection\sample14-direct-import-settings-for-rdo.ps1"

    # . "$global:rootDir\SamplesCollection\sample15-read-import-rdo-settings.ps1"

    # . "$global:rootDir\SamplesCollection\sample16-read-import-document-settings.ps1"

    # . "$global:rootDir\SamplesCollection\sample17-read-import-jobs.ps1"

    # . "$global:rootDir\SamplesCollection\sample18-get-data-source.ps1"

    # . "$global:rootDir\SamplesCollection\sample19-get-import-job-details-and-progress.ps1"

    # . "$global:rootDir\SamplesCollection\sample20-get-data-source-details-and-progress.ps1"

    # . "$global:rootDir\SamplesCollection\sample21-cancel-started-job.ps1"

    # . "$global:rootDir\SamplesCollection\sample22-read-response.ps1"

    # . "$global:rootDir\SamplesCollection\sample23-get-data-source-errors.ps1"
}