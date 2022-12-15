class Auth {
	[String] $url
	[String] $user
	[String] $password
	[String] $authToken

	Auth($url, $user, $pass) {
		$this.url = $url
		$this.user = $user
		$this.password = $pass

		$pair = $this.user + ":" + $this.password
		$this.authToken = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
	}

	[Object] getHeader(){
		$header = @{
			"Accept" = "*/*"
			"Authorization" = "Basic " + $this.authToken
			"Content-Type" = "application/json"
			"X-CSRF-Header" = "-"
		}
	
		return $header
	}
}
