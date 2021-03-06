#mostly inspired by http://github.com/Constellation/ruby-net-github-upload
Add-Type @'
public class GitHubFile
{
    public string id ;    
    public string name;
    public string description;
    public string date;
    public string downloads;
    public string size;
}
'@

function downloads_for_github_repo($login, $repo){
    [System.Reflection.Assembly]::LoadFrom((get-item "lib\html.agility.pack\HtmlAgilityPack.dll"))
    
    $downloads_path = "http://github.com/"+$login+"/"+$repo+"/downloads"

    $wc = new-object net.webclient
    $downloads_html = $wc.DownloadString($downloads_path)
    
    $html = new-object HtmlAgilityPack.HtmlDocument
    $html.loadhtml($downloads_html)

    $downloads = ($html.DocumentNode.SelectNodes("//table[@id='s3_downloads']/tr[@id]"))
    return $downloads | foreach-object { file_from_tr $_ }
}

function file_from_tr($tr){
    $github_file = new-object GitHubFile

    $github_file.id = $tr.GetAttributeValue("id","").Replace("download_","")
    $github_file.name = $tr.SelectNodes("./td/a")[0].InnerText
    $github_file.description = $tr.SelectNodes("./td")[2].InnerText
    $github_file.date = $tr.SelectNodes("./td")[3].InnerText
    $github_file.downloads = $tr.SelectNodes("./td")[4].InnerText
    $github_file.size = $tr.SelectNodes("./td")[5].InnerText
    
    return $github_file
}


function upload_file_to_github($login, $repo, $api_key, $file, $filename, $description){
    [void][System.Reflection.Assembly]::LoadFrom((get-item "lib\Krystalware.UploadHelper.dll"))
    
    $full_repo = $login+"/"+$repo
    $downloads_path = "http://github.com/"+$full_repo+"/downloads"
    
    $post = new-object System.Collections.Specialized.NameValueCollection
    $post.Add('login',$login)
    $post.Add('token',$api_key)
    $post.Add('file_size',$file.Length)
    $post.Add('content_type',"application/octet-stream")
    $post.Add('file_name',$filename)
    $post.Add('description',$description)
    $wc = new-object net.webclient
    $upload_info = [xml][System.Text.Encoding]::ASCII.GetString($wc.UploadValues($downloads_path, $post))
    
    $post = new-object System.Collections.Specialized.NameValueCollection
    $post.Add('FileName',$filename)
    $post.Add('policy',$upload_info.hash.policy)
    $post.Add('success_action_status',"201")
    $post.Add('key',$upload_info.hash.prefix+$file.Name)
    $post.Add('AWSAccessKeyId',$upload_info.hash.accesskeyid)
    $post.Add('signature',$upload_info.hash.signature)
    $post.Add('acl',$upload_info.hash.acl)
    
    $upload_file = new-object Krystalware.UploadHelper.UploadFile $file.FullName, "file", "application/octet-stream" 
    [void][Krystalware.UploadHelper.HttpUploadHelper]::Upload("http://github.s3.amazonaws.com/", $upload_file, $post)
}

function test{
    $login = "brunomlopes"
    $repo = "Blaze-IronPythonPlugins"
    $api_key = get-content "api.key"

    $filename = "e:\temp.txt"
    $description = "This is a test file!"
    $file = get-item $filename

    upload_file_to_github $login $repo $api_key $file $file.Name $description
}