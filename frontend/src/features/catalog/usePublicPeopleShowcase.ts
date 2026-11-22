/**
 * @date 2026-11-22
 * @purpose Shared query hook providing database-backed public student portraits and validated offline fallbacks.
 */
import { useQuery } from "@tanstack/react-query";
import { studentsApi, type PublicStudentShowcaseItem } from "@/api";
import { queryKeys } from "@/lib/queryKeys";
import { isSafeAssetOrHttpUrl, resolveMediaUrl } from "@/lib/urlValidation";

export const FALLBACK_SHOWCASE_STUDENTS: readonly PublicStudentShowcaseItem[] = [
  {
    studentId: 101,
    firstName: "Salma",
    lastName: "Khaled",
    academicYear: 4,
    facultyName: "Faculty of Computers and Artificial Intelligence",
    facultyCode: "FCAI",
    universityName: "Cairo University",
    universityLogoUrl:
      "https://upload.wikimedia.org/wikipedia/commons/3/3c/Cairo_University_new_logo.png",
    bio: "Building cross-faculty AI & React squads at Cairo University FCAI.",
    photoUrl: "/images/people/salma-khaled.jpg",
    totalPoints: 2840,
    level: 6,
    badgesCount: 3,
    topSkills: ["Machine Learning", "Python", "React"],
  },
  {
    studentId: 102,
    firstName: "Omar",
    lastName: "Mostafa",
    academicYear: 3,
    facultyName: "Faculty of Engineering",
    facultyCode: "ENG_CAPU",
    universityName: "Capital University",
    universityLogoUrl:
      "https://upload.wikimedia.org/wikipedia/commons/c/c1/Capital_University_Logo.png",
    bio: "Embedded systems & full-stack clubs lead at Capital University.",
    photoUrl: "/images/people/omar-mostafa.jpg",
    totalPoints: 2490,
    level: 5,
    badgesCount: 3,
    topSkills: ["C#", "Docker", "TypeScript"],
  },
  {
    studentId: 103,
    firstName: "Youssef",
    lastName: "Adel",
    academicYear: 3,
    facultyName: "Faculty of Computer and Information Sciences",
    facultyCode: "CIS",
    universityName: "Ain Shams University",
    universityLogoUrl: "https://upload.wikimedia.org/wikipedia/en/3/3d/Ain_Shams_logo.png",
    bio: "Backend architect & campus workshop mentor at Ain Shams CIS.",
    photoUrl: "/images/people/youssef-adel.jpg",
    totalPoints: 2310,
    level: 5,
    badgesCount: 3,
    topSkills: ["ASP.NET Core", "Docker", "PostgreSQL"],
  },
  {
    studentId: 104,
    firstName: "Hana",
    lastName: "El-Sayed",
    academicYear: 2,
    facultyName: "Faculty of Computing and Data Science",
    facultyCode: "CDS_ALEX",
    universityName: "Alexandria University",
    universityLogoUrl:
      "https://upload.wikimedia.org/wikipedia/ar/6/63/%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%A7%D9%84%D8%A5%D8%B3%D9%83%D9%86%D8%AF%D8%B1%D9%8A%D8%A9.png",
    bio: "Data science & computer vision researcher at Alexandria CDS.",
    photoUrl: "/images/people/hana-el-sayed.jpg",
    totalPoints: 1920,
    level: 5,
    badgesCount: 2,
    topSkills: ["Data Analysis", "Figma", "Python"],
  },
  {
    studentId: 105,
    firstName: "Karim",
    lastName: "Mansour",
    academicYear: 4,
    facultyName: "Faculty of Media Engineering and Technology",
    facultyCode: "MET_GUC",
    universityName: "German University in Cairo (GUC)",
    universityLogoUrl: "https://www.guc.edu.eg/img/guc_logo_og.png",
    bio: "Systems engineer & design-systems builder at GUC MET.",
    photoUrl: "/images/people/karim-mansour.jpg",
    totalPoints: 1780,
    level: 5,
    badgesCount: 2,
    topSkills: ["React", "TypeScript", "UI/UX Design"],
  },
  {
    studentId: 106,
    firstName: "Mariam",
    lastName: "Nabil",
    academicYear: 2,
    facultyName: "Faculty of Computers and Information",
    facultyCode: "FCI_MANS",
    universityName: "Mansoura University",
    universityLogoUrl:
      "https://upload.wikimedia.org/wikipedia/ar/5/5e/%D8%B4%D8%B9%D8%A7%D8%B1_%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%A7%D9%84%D9%85%D9%86%D8%B5%D9%88%D8%B1%D8%A9.png",
    bio: "Applied ML & UX researcher at Mansoura University FCI.",
    photoUrl: "/images/people/mariam-nabil.jpg",
    totalPoints: 1540,
    level: 4,
    badgesCount: 2,
    topSkills: ["Machine Learning", "Python", "UI/UX Design"],
  },
];

export function usePublicPeopleShowcase(limit = 6, enabled = true) {
  const query = useQuery({
    queryKey: queryKeys.students.showcase(limit),
    queryFn: ({ signal }) => studentsApi.getPublicShowcase(limit, signal),
    staleTime: 5 * 60 * 1000,
    retry: false,
    enabled,
  });

  const apiStudents =
    Array.isArray(query.data) && query.data.length > 0
      ? query.data
          .filter((item) => isSafeAssetOrHttpUrl(item.photoUrl))
          .map((item) => ({
            ...item,
            photoUrl: resolveMediaUrl(item.photoUrl) ?? item.photoUrl,
          }))
      : [];

  const students =
    apiStudents.length > 0 ? apiStudents : FALLBACK_SHOWCASE_STUDENTS.slice(0, limit);

  return {
    students,
    isLiveFromDatabase: apiStudents.length > 0,
    isLoading: query.isPending,
  };
}
