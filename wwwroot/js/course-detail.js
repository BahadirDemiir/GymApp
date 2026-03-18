function showCoachBio(bio) {
  alert(bio);
}

document.addEventListener('DOMContentLoaded', function() {
  const enrollBtn = document.querySelector('.btn-soft');
  if (enrollBtn) {
    enrollBtn.addEventListener('click', function(e) {
      e.preventDefault();
      alert('Kayıt işlemi başlatılıyor...');
    });
  }
  
  const favoriteBtn = document.querySelector('.btn-ghost');
  if (favoriteBtn) {
    favoriteBtn.addEventListener('click', function(e) {
      e.preventDefault();
      alert('Favorilere eklendi!');
    });
  }
});