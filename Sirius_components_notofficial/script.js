/* limita i caratteri dopo il 20

var e=document.querySelectorAll('option')
e.forEach(x=>{
if(x.textContent.length>20)
x.textContent=x.textContent.substring(0,20)+'...';
}) */
