if (Get-Command Register-ArgumentCompleter -ErrorAction Ignore) {

	# $scriptblock is a fn that takes 
	#     param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)

	# Register-ArgumentCompleter $commandName $parameterName $scriptblock $description 

}