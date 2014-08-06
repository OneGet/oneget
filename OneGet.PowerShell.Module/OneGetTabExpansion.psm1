if (Get-Command Register-ArgumentCompleter -ErrorAction Ignore) {

	# $scriptblock is a fn that takes 
	#     param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)

	# Register-ArgumentCompleter $commandName $parameterName $scriptblock $description 

	Register-ArgumentCompleter -CommandName "Find-Package" -ParameterName "Provider" -ScriptBlock -Description "oneget-completer" -ScriptBlock { 
		param(            
			$commandName,             
			$parameterName,             
			$wordToComplete,             
			$commandAst,             
			$fakeBoundParameter            
		)            
		[System.Console]::Beep(1000,100) 

		$Lista = @( 'XX','AA' )

		foreach ($Typ in $Lista) {            
			New-CompletionResult -CompletionText $Typ -ToolTip $Typ            
		}
         

	}

}