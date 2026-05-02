using System.Collections.Generic;

public static class CelestialCatalog
{
    public class CatalogEntry
    {
        public string name;
        public string category;      // "Constellation", "Planet", "Moon", "Exoplanet", etc
        public string parentName;    // e.g. "Jupiter" for its moons
        public string description;
        public float ra;
        public float dec;
        public bool  isDynamic;      // true = position changes (planets, sun, moon)
    }

    public static readonly Dictionary<string, string> PlanetDescriptions 
        = new Dictionary<string, string>()
    {
        { "Sun", "The Sun is the star at the center of our solar system. A nearly perfect sphere of hot plasma, it accounts for 99.86% of the total mass of the solar system. Its core temperature reaches 15 million °C, where nuclear fusion converts hydrogen into helium, releasing the energy that sustains all life on Earth." },

        { "Mercury", "Mercury is the smallest planet and closest to the Sun. A day on Mercury lasts 59 Earth days, but a year is only 88 days. Without a meaningful atmosphere, surface temperatures swing from -180°C at night to 430°C during the day — the most extreme temperature range of any planet." },

        { "Venus", "Venus is the hottest planet in the solar system despite not being the closest to the Sun. Its thick atmosphere of CO₂ traps heat in a runaway greenhouse effect, keeping surface temperatures at a crushing 465°C. It rotates backwards compared to most planets, meaning the Sun rises in the west." },

        { "Moon", "Earth's Moon is the fifth largest natural satellite in the solar system. It formed approximately 4.5 billion years ago, likely from debris ejected when a Mars-sized body collided with the early Earth. The Moon stabilizes Earth's axial tilt, moderating our climate over long timescales." },

        { "Mars", "Mars is a cold desert world with the largest volcano in the solar system — Olympus Mons, nearly three times the height of Everest. Evidence of ancient river valleys and lake beds suggests liquid water once flowed on its surface. Mars has two small moons: Phobos and Deimos." },

        { "Jupiter", "Jupiter is the largest planet — more than twice the mass of all other planets combined. Its Great Red Spot is a storm that has raged for at least 350 years. Jupiter has 95 known moons, including Ganymede, the largest moon in the solar system, which is bigger than Mercury." },

        { "Saturn", "Saturn is renowned for its spectacular ring system, made mostly of ice and rock fragments ranging from tiny grains to chunks the size of houses. Despite being the second largest planet, it is the least dense — it would float in water. Saturn has 146 known moons, more than any other planet." },

        { "Uranus", "Uranus rotates on its side with an axial tilt of 98°, meaning its poles receive more sunlight than its equator. It is an ice giant composed primarily of water, methane and ammonia ices. Its blue-green color comes from methane in its atmosphere absorbing red light." },

        { "Neptune", "Neptune is the windiest planet, with storms reaching 2,100 km/h — the fastest winds in the solar system. It was the first planet located through mathematical prediction rather than direct observation. Its largest moon Triton orbits backwards and is likely a captured Kuiper Belt object." }
    };

    public static readonly Dictionary<string, string> MoonDescriptions
        = new Dictionary<string, string>()
    {
        { "Phobos",   "Mars' larger and inner moon. It orbits so close to Mars that it completes an orbit in just 7 hours — faster than Mars rotates. It will eventually break apart or crash into Mars within 50 million years." },
        { "Deimos",   "Mars' smaller and outer moon. Smooth and featureless compared to Phobos, it is one of the least reflective bodies in the solar system." },
        { "Io",       "Jupiter's innermost Galilean moon and the most volcanically active body in the solar system. Tidal forces from Jupiter constantly flex its interior, generating enormous heat." },
        { "Europa",   "Jupiter's moon Europa has a global saltwater ocean beneath its icy crust. It is considered one of the most promising places to search for extraterrestrial life." },
        { "Ganymede", "The largest moon in the solar system, bigger than Mercury. It is the only moon known to have its own magnetic field." },
        { "Callisto", "Jupiter's outermost Galilean moon. Its ancient, heavily cratered surface has changed little in billions of years." },
        { "Titan",    "Saturn's largest moon has a thick nitrogen atmosphere and lakes of liquid methane on its surface — the only world besides Earth with stable surface liquids." },
        { "Enceladus","Saturn's moon actively vents water vapor and ice particles from its south pole into space, feeding Saturn's E ring. A subsurface ocean makes it a strong candidate for life." },
        { "Triton",   "Neptune's largest moon orbits backwards. It has geysers of nitrogen gas erupting from its surface and is slowly spiraling inward toward Neptune." },
        { "Charon",   "Pluto's largest moon is so big relative to Pluto that they are often considered a double dwarf planet system." }
    };

    public static readonly Dictionary<string, string> ExoplanetDescriptions
        = new Dictionary<string, string>()
    {
        { "Proxima Centauri b", "The closest known exoplanet to Earth at just 4.2 light-years away, orbiting in the habitable zone of Proxima Centauri. Whether it can support life depends on whether it has held onto an atmosphere against stellar flares." },
        { "51 Pegasi b",        "The first exoplanet discovered orbiting a Sun-like star (1995). A hot Jupiter orbiting its star every 4 days, it fundamentally changed our understanding of how planetary systems form." },
        { "HD 209458 b",        "Known as Osiris, this was the first exoplanet observed transiting its star and the first found to have an evaporating atmosphere, with hydrogen streaming away into space." },
        { "Kepler-22b",         "One of the first exoplanets confirmed in a star's habitable zone by the Kepler mission. It is roughly 2.4 times Earth's radius, but whether it is rocky, oceanic or gaseous remains unknown." },
        { "TRAPPIST-1b",        "Part of the remarkable TRAPPIST-1 system with seven Earth-sized planets. Three of them orbit in the habitable zone, making this system a top target for atmospheric studies with the James Webb Space Telescope." },
        { "55 Cancri e",        "A super-Earth so close to its star that a year lasts only 18 hours. Surface temperatures may be hot enough to melt rock, and early observations suggested it could have a carbon-rich interior — possibly diamond." },
        { "Kepler-442b",        "One of the most Earth-like exoplanets known, with a similarity index of 0.84 compared to Earth's 1.0. It orbits a cooler star than our Sun and receives about 70% of Earth's sunlight." },
        { "HD 40307 g",         "A super-Earth in the habitable zone of an orange dwarf star. Unlike many habitable zone candidates it is far enough from its star that it is unlikely to be tidally locked." },
        { "Tau Ceti e",         "Orbiting one of the nearest Sun-like stars at 11.9 light-years away. Tau Ceti is similar to our Sun in size and brightness, making its planets interesting targets in the search for life." },
        { "GJ 667C c",          "A super-Earth orbiting in the habitable zone of a red dwarf in a triple star system. It receives similar energy from its star as Earth does from the Sun." }
    };
}