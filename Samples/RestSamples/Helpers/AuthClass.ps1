class Auth {
	[String] $url
	[String] $user
	[String] $password
	[String] $base64AuthenticationString

	Auth($url, $user, $pass) {
		$this.url = $url
		$this.user = $user
		$this.password = $pass

		# Basic authentication method was applied for samples purpose.
		# See Relativity REST API authentication documentation describing other authentication methods.
		# https://platform.relativity.com/RelativityOne/Content/REST_API/REST_API_authentication.htm
		$pair = $this.user + ":" + $this.password
		$this.base64AuthenticationString = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
	}

	# See Relativity REST API HTTP headers documentaton: https://platform.relativity.com/RelativityOne/Content/REST_API/HTTP_headers.htm
	[Object] getHeader(){
		$header = @{
			"Accept" = "*/*"
			"Authorization" = "Basic " + $this.base64AuthenticationString
			"Content-Type" = "application/json"
			"X-CSRF-Header" = "-"
		}
	
		return $header
	}
}
