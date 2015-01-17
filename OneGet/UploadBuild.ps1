## This just zips up the relavent binaries
ipmo 'C:\Program Files (x86)\Outercurve Foundation\Modules\CoApp\CoApp.psd1'
ren Install-OneGet.exe install-oneget.exe

copy-itemex -force install-oneget.exe  oneget:providers\
copy-itemex -force install-oneget.exe  oneget:providers\Install-OneGet.exe

# send-tweet -Message "Posted new #OneGet *Experimental* build https://oneget.org/$n"
echo build at https://oneget.org/install-oneget.exe 