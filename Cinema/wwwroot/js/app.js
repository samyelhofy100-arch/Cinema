// ─── CINEPLEX STATE MANAGER ───
const Cinema = {
  state: {
    film: null,
    showtime: null,
    date: null,
    seats: [],
    addons: {},
    paymentMethod: null
  },

  save() {
    sessionStorage.setItem('cinema_state', JSON.stringify(this.state));
  },

  load() {
    const saved = sessionStorage.getItem('cinema_state');
    if (saved) this.state = JSON.parse(saved);
    return this.state;
  },

  setFilm(film, showtime, date) {
    this.state.film = film;
    this.state.showtime = showtime;
    this.state.date = date;
    this.save();
  },

  setSeats(seats) {
    this.state.seats = seats;
    this.save();
  },

  setAddons(addons) {
    this.state.addons = addons;
    this.save();
  },

  setPayment(method) {
    this.state.paymentMethod = method;
    this.save();
  },

  getTotal() {
    const s = this.state;
    let total = 0;
    if (s.seats.length > 0 && s.film) {
      s.seats.forEach(seat => {
        total += seat.type === 'vip' ? s.film.priceVip : s.film.priceRegular;
      });
    }
    Object.values(s.addons || {}).forEach(addon => {
      total += addon.price * addon.qty;
    });
    return total;
  },

  formatRp(num) {
    return 'Rp ' + num.toLocaleString('id-ID');
  },

  generateCode() {
    return 'CPX' + Math.random().toString(36).substr(2, 6).toUpperCase();
  }
};

// ─── FILMS DATA ───
const FILMS = [
  {
    id: 1,
    title: "DUNE: PART TWO",
    genre: "Sci-Fi",
    duration: "155 min",
    rating: "13+",
    imdb: "8.8",
    badge: "NOW SHOWING",
    badgeType: "",
    poster: "https://image.tmdb.org/t/p/w500/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg",
    priceRegular: 55000,
    priceVip: 85000,
    showtimes: ["10:00", "13:30", "16:00", "19:30", "22:00"],
    dates: ["Hari ini", "Besok", "Minggu"],
    desc: "Paul Atreides bersatu dengan Fremen untuk memimpin perlawanan melawan kekuatan gelap demi masa depan Arrakis."
  },
  {
    id: 2,
    title: "OPPENHEIMER",
    genre: "Drama",
    duration: "180 min",
    rating: "17+",
    imdb: "8.9",
    badge: "TRENDING",
    badgeType: "",
    poster: "https://image.tmdb.org/t/p/w500/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg",
    priceRegular: 60000,
    priceVip: 90000,
    showtimes: ["11:00", "14:30", "18:00", "21:30"],
    dates: ["Hari ini", "Besok", "Minggu"],
    desc: "Kisah J. Robert Oppenheimer dan perannya dalam proyek bom atom Manhattan yang mengubah dunia."
  },
  {
    id: 3,
    title: "AQUAMAN 2",
    genre: "Action",
    duration: "124 min",
    rating: "13+",
    imdb: "6.5",
    badge: "NEW",
    badgeType: "new",
    poster: "https://image.tmdb.org/t/p/w500/7lTnfelity0YFdvkaaxue5sHliI.jpg",
    priceRegular: 50000,
    priceVip: 80000,
    showtimes: ["09:00", "12:00", "15:00", "18:30", "21:00"],
    dates: ["Hari ini", "Besok", "Minggu"],
    desc: "Arthur Curry dan saudaranya harus bersatu menghadapi ancaman baru yang mengguncang Atlantis."
  },
  {
    id: 4,
    title: "INSIDE OUT 2",
    genre: "Animation",
    duration: "100 min",
    rating: "SU",
    imdb: "7.8",
    badge: "FAV",
    badgeType: "new",
    poster: "https://image.tmdb.org/t/p/w500/vpnVM9B6NMmQpWeZvzLvDESb2QY.jpg",
    priceRegular: 45000,
    priceVip: 75000,
    showtimes: ["09:30", "11:30", "14:00", "16:00", "19:00"],
    dates: ["Hari ini", "Besok", "Minggu"],
    desc: "Riley memasuki masa remaja dan emosi-emosi baru muncul mengubah segalanya di dalam kepalanya."
  },
  {
    id: 5,
    title: "INTERSTELLAR",
    genre: "Sci-Fi",
    duration: "169 min",
    rating: "13+",
    imdb: "8.7",
    badge: "CLASSIC",
    badgeType: "",
    poster: "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
    priceRegular: 65000,
    priceVip: 95000,
    showtimes: ["13:00", "17:00", "21:00"],
    dates: ["Hari ini", "Besok"],
    desc: "Tim astronaut menembus lubang cacing untuk mencari planet baru demi kelangsungan hidup manusia."
  },
  {
    id: 6,
    title: "AVATAR 2",
    genre: "Fantasy",
    duration: "192 min",
    rating: "13+",
    imdb: "7.6",
    badge: "3D IMAX",
    badgeType: "",
    poster: "https://image.tmdb.org/t/p/w500/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg",
    priceRegular: 75000,
    priceVip: 110000,
    showtimes: ["10:30", "14:30", "19:00"],
    dates: ["Hari ini", "Besok", "Minggu"],
    desc: "Jake dan Neytiri menjelajahi lautan Pandora sambil berjuang mempertahankan keluarga dari ancaman."
  }
];

// ─── ADDONS DATA ───
const ADDONS = [
  { id: 'combo1', name: 'Combo Pasangan', desc: '2 Popcorn Large + 2 Cola Large', price: 130000, img: 'https://images.unsplash.com/photo-1585647347483-22b66260dfff?w=400&h=220&fit=crop&auto=format' },
  { id: 'combo2', name: 'Combo Keluarga', desc: '4 Popcorn + 4 Cola + 2 Nachos', price: 250000, img: 'https://images.unsplash.com/photo-1606144042614-b2417e99c4e3?w=400&h=220&fit=crop&auto=format' },
  { id: 'cola_m', name: 'Cola Medium', desc: 'Minuman soda dingin 450ml', price: 25000, img: 'https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=400&h=220&fit=crop&auto=format' },
  { id: 'cola_l', name: 'Cola Large', desc: 'Minuman soda dingin 600ml + straw', price: 35000, img: 'https://images.unsplash.com/photo-1520201163981-8cc95007dd2a?w=400&h=220&fit=crop&auto=format' },
  { id: 'mineral', name: 'Air Mineral', desc: 'Aqua botol 600ml', price: 15000, img: 'https://images.unsplash.com/photo-1548839140-29a749e1cf4d?w=400&h=220&fit=crop&auto=format' },
  { id: 'popcorn_lg', name: 'Popcorn Large', desc: 'Popcorn buttery jumbo 120g + refill', price: 45000, img: 'https://images.unsplash.com/photo-1578849278619-e73505e9610f?w=400&h=220&fit=crop&auto=format' },
  { id: 'nachos', name: 'Nachos + Dip', desc: 'Nachos crispy dengan salsa & cheese dip', price: 45000, img: 'https://images.unsplash.com/photo-1513456852971-30c0b8199d4d?w=400&h=220&fit=crop&auto=format' },
  { id: 'hotdog', name: 'Hot Dog Jumbo', desc: 'Hot dog beef dengan mustard & mayo', price: 40000, img: 'https://images.unsplash.com/photo-1612392062631-94b06e0572be?w=400&h=220&fit=crop&auto=format' },
  { id: 'popcorn_reg', name: 'Popcorn Regular', desc: 'Popcorn buttery klasik 70g', price: 35000, img: 'https://images.unsplash.com/photo-1587654780291-39c9404d746b?w=400&h=220&fit=crop&auto=format' },
  { id: 'choco', name: 'Lava Cake', desc: 'Chocolate lava cake hangat', price: 38000, img: 'https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=400&h=220&fit=crop&auto=format' }
];

// ─── NAVBAR BUILDER ───
function buildNav(activeStep) {
  const steps = [
    { num: 1, label: 'Pilih Film', href: 'index.html' },
    { num: 2, label: 'Pilih Kursi', href: 'seats.html' },
    { num: 3, label: 'Add-On', href: 'addons.html' },
    { num: 4, label: 'Bayar', href: 'payment.html' }
  ];

  const stepsHtml = steps.map((s, i) => {
    let cls = '';
    if (s.num === activeStep) cls = 'active';
    else if (s.num < activeStep) cls = 'done';
    const divider = i < steps.length - 1 ? '<div class="step-divider"></div>' : '';
    const icon = s.num < activeStep ? '✓' : s.num;
    return `<div class="step ${cls}"><div class="step-num">${icon}</div><span>${s.label}</span></div>${divider}`;
  }).join('');

  return `
    <nav>
      <a href="index.html" class="logo">CINE<span>PLEX</span></a>
      <div class="steps">${stepsHtml}</div>
      <div style="width:120px"></div>
    </nav>
  `;
}
