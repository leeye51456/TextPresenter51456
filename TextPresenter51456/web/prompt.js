var xhr = new XMLHttpRequest();
var pgmSection, pvwSection, nextpgSection;
var session = 0, status = 0;


function xhrReadyStateChange() {
  if (xhr.readyState === XMLHttpRequest.DONE) {
    if (xhr.status === 200) {
      try {
        var obj = JSON.parse(xhr.responseText);
      } catch (e) {
        console.log('xhrReadyStateChange: JSON parse error');
        return;
      }
      if (obj.session === undefined) {
        console.log('xhrReadyStateChange: "session" property does NOT exist');
        return;
      }
      if (obj.session === 0) { // no change
        xhr.open('POST', 'prompt', true);
        xhr.send('session=' + session + '&status=' + status);
        return;
      }
      if (obj.status === undefined) {
        console.log('xhrReadyStateChange: "status" property does NOT exist');
        return;
      }
      session = obj.session;
      status = obj.status;
      if (obj.pgm === undefined) {
        console.log('xhrReadyStateChange: "pgm" property does NOT exist');
      } else {
        pgmSection.innerHTML = obj.pgm.replace(/\n/g, '<br>');
      }
      if (obj.pvw === undefined) {
        console.log('xhrReadyStateChange: "pvw" property does NOT exist');
      } else {
        pvwSection.innerHTML = obj.pvw.replace(/\n/g, '<br>');
      }
      if (obj.next === undefined) {
        console.log('xhrReadyStateChange: "next" property does NOT exist');
      } else {
        nextpgSection.innerHTML = obj.next.replace(/\n/g, '<br>');
      }
      xhr.open('POST', 'prompt', true);
      xhr.send('session=' + session + '&status=' + status);
    }
  }
}

function init() {
  xhr.onreadystatechange = xhrReadyStateChange;

  pgmSection = document.getElementById('pgm-section');
  pvwSection = document.getElementById('pvw-section');
  nextpgSection = document.getElementById('nextpg-section');

  xhr.open('POST', 'prompt', true);
  xhr.send('session=0&status=0');
}
window.addEventListener('load', init);
