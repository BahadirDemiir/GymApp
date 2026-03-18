
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymAppFresh.Models;
using GymAppFresh.Models.ViewModel;
using GymAppFresh.Utils;
using Microsoft.EntityFrameworkCore;

namespace GymAppFresh.Services
{
    public class RecommenderService
    {
        private readonly DataContext _db;
        public RecommenderService(DataContext db) => _db = db;

        public async Task<List<ProgramSuggestionVm>> SuggestForMemberAsync(Member m)
        {
            var bmi = m.Weight.HasValue && m.Height.HasValue ? FitnessCalc.Bmi(m.Weight.Value, m.Height.Value) : null;                 
            var age = m.BirthDate.HasValue ? FitnessCalc.Age(m.BirthDate.Value) : null;                      
            int? goal = m.FitnessGoal.HasValue ? (int)m.FitnessGoal.Value : (int?)null;
            int? prefDays = m.daysPerWeek.HasValue ? (int)m.daysPerWeek.Value : (int?)null;

            var programs = await _db.WorkoutPrograms.AsNoTracking().ToListAsync();
            var list = new List<ProgramSuggestionVm>();

            foreach (var p in programs)
            {
                int s = 40; // base score (düşürdük)
                var why = new List<string>();

                // 1) Hedef eşleşmesi (en önemli - %35)
                if (goal.HasValue && p.Goal == goal.Value)
                {
                    s += 35; 
                    why.Add(" Hedefinle mükemmel eşleşme");
                }
                else if (goal.HasValue)
                {
                    s -= 15; // daha büyük ceza
                }

                // 2) Gün sayısı uyumu (%25)
                if (prefDays.HasValue)
                {
                    int dayDiff = Math.Abs(p.DaysPerWeek - prefDays.Value);
                    if (dayDiff == 0)
                    {
                        s += 25; 
                        why.Add("Gün sayısı tam uyumlu");
                    }
                    else if (dayDiff == 1)
                    {
                        s += 15; 
                        why.Add("Gün sayısı yakın");
                    }
                    else if (dayDiff == 2)
                    {
                        s += 5; 
                        why.Add("Gün sayısı kabul edilebilir");
                    }
                    else
                    {
                        s -= 10;
                    }
                }

                // 3) Yaş faktörü (%15)
                if (age.HasValue)
                {
                    if (age.Value < 25 && p.Level.Equals("Beginner", StringComparison.OrdinalIgnoreCase))
                    {
                        s += 15; 
                        why.Add("Genç yaş için ideal başlangıç");
                    }
                    else if (age.Value >= 25 && age.Value < 40)
                    {
                        s += 10; 
                        why.Add("Orta yaş için uygun");
                    }
                    else if (age.Value >= 40 && p.Level.Equals("Beginner", StringComparison.OrdinalIgnoreCase))
                    {
                        s += 20; 
                        why.Add("Güvenli başlangıç seviyesi");
                    }
                }

                // 4) BMI ve hedef kombinasyonu (%15)
                if (bmi.HasValue && goal.HasValue)
                {
                    // Kilo verme hedefleri için
                    if (goal == (int)FitnessGoal.LoseWeight || goal == (int)FitnessGoal.LoseFat)
                    {
                        if (bmi >= 30 && p.Split.Equals("FullBody", StringComparison.OrdinalIgnoreCase))
                        {
                            s += 15; 
                            why.Add($"Yüksek BMI ({bmi:0.0}) için ideal full body program");
                        }
                        else if (bmi >= 25 && bmi < 30)
                        {
                            s += 10; 
                            why.Add("Kilo verme için uygun program");
                        }
                    }
                    
                    // Kas geliştirme hedefleri için
                    if (goal == (int)FitnessGoal.GainMuscle)
                    {
                        if (p.Split.Equals("UpperLower", StringComparison.OrdinalIgnoreCase) ||
                            p.Split.Equals("PPL", StringComparison.OrdinalIgnoreCase))
                        {
                            s += 15; 
                            why.Add("Kas geliştirme için ideal split");
                        }
                    }

                    // Kilo alma hedefleri için
                    if (goal == (int)FitnessGoal.GainWeight && p.DaysPerWeek >= 4)
                    {
                        s += 10; 
                        why.Add("Kilo alma hedefinde 4+ gün avantaj");
                    }

                    // Sağlık iyileştirme hedefleri için
                    if (goal == (int)FitnessGoal.ImproveHealth)
                    {
                        if (p.Level.Equals("Beginner", StringComparison.OrdinalIgnoreCase))
                        {
                            s += 12; 
                            why.Add("Sağlık için güvenli başlangıç");
                        }
                    }
                }

                // 5) Cinsiyet faktörü (%5)
                if (m.Gender.HasValue)
                {
                    if (m.Gender == Gender.Female && 
                        (p.Split.Equals("UpperLower", StringComparison.OrdinalIgnoreCase) ||
                         p.Level.Equals("Beginner", StringComparison.OrdinalIgnoreCase)))
                    {
                        s += 5; 
                        why.Add("Kadınlar için uygun program");
                    }
                    else if (m.Gender == Gender.Male && 
                             p.Split.Equals("PPL", StringComparison.OrdinalIgnoreCase))
                    {
                        s += 5; 
                        why.Add("Erkekler için popüler split");
                    }
                }

                // 6) Seviye bonusu (%5)
                if (p.Level.Equals("Beginner", StringComparison.OrdinalIgnoreCase))
                {
                    s += 5; 
                    why.Add("️ Güvenli başlangıç seviyesi");
                }

                // 7) Ek optimizasyonlar
                // Eğer hiç uyum yoksa minimum puan ver
                if (why.Count == 0)
                {
                    s = Math.Max(s, 20); // minimum 20 puan
                    why.Add("Genel fitness programı");
                }

                s = Math.Clamp(s, 0, 100);
                list.Add(new ProgramSuggestionVm { 
                    Program = p, 
                    Score = s, 
                    Why = string.Join(" • ", why) 
                });
            }

            return list.OrderByDescending(x => x.Score).Take(3).ToList();
        }
    }
}
