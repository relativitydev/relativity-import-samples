class WebRequest {
    [Object] $url
    [Object] $header

    WebRequest([Auth]$auth){
        $this.url = $auth.url
        $this.header = $auth.getHeader()
    }

    [Object] callPost($uri, $body){
        $serviceUrl = $this.url + $uri
        Write-Information -MessageData "POST request url $serviceUrl" -InformationAction Continue
        Write-Information -MessageData "Body $body" -InformationAction Continue
        
        $response = try { 
            Invoke-RestMethod -Uri $serviceUrl -Method 'Post' -Headers $this.header -Body $body
        } catch { 
            $exception = $_."Exception"
            $response = $_."Exception"."Response"
            $details = $_."ErrorDetails"."Message"
            Write-Information -MessageData "Exception $exception" -InformationAction Continue
            Write-Information -MessageData "Exception Response $response" -InformationAction Continue
            Write-Information -MessageData "Error Details Message $details" -InformationAction Continue
            throw
        }
        
        return $response
    }

    [Object] callGet($uri){
        $serviceUrl = $this.url + $uri
        
        $response = try { 
            Invoke-RestMethod -Uri $serviceUrl -Method 'Get' -Headers $this.header
        } catch {
            $exception = $_."Exception"
            $response = $_."Exception"."Response"
            $details = $_."ErrorDetails"."Message"
            Write-Information -MessageData "Exception $exception" -InformationAction Continue
            Write-Information -MessageData "Exception Response $response" -InformationAction Continue
            Write-Information -MessageData "Error Details Message $details" -InformationAction Continue
            throw
        }
        
        return $response
    }

    check($response){
        if($response."IsSuccess" = $false)
        {
            Write-Information -MessageData "Response doesn't idicate success" -InformationAction Continue
            exit 1
        }
    }
}