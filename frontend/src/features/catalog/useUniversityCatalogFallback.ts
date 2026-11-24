/**
 * @date 2026-11-22
 * @purpose Shared query hook and official database fallback catalog for partner universities and official seal lookups.
 */
import { useQuery } from "@tanstack/react-query";
import { useMemo } from "react";
import { universitiesApi, type UniversityCatalogItem } from "@/api";
import { queryKeys } from "@/lib/queryKeys";

export const FALLBACK_UNIVERSITIES: UniversityCatalogItem[] = [
  {
    universityId: 1,
    name: "Cairo University",
    city: "Giza",
    country: "Egypt",
    website: "https://cu.edu.eg",
    logoUrl: "https://upload.wikimedia.org/wikipedia/commons/3/3c/Cairo_University_new_logo.png",
    faculties: [
      { facultyId: 1, name: "Faculty of Computers and Artificial Intelligence", code: "FCAI" },
      { facultyId: 2, name: "Faculty of Engineering", code: "ENG" },
    ],
  },
  {
    universityId: 2,
    name: "Ain Shams University",
    city: "Cairo",
    country: "Egypt",
    website: "https://asu.edu.eg",
    logoUrl: "https://upload.wikimedia.org/wikipedia/en/3/3d/Ain_Shams_logo.png",
    faculties: [
      { facultyId: 3, name: "Faculty of Computer and Information Sciences", code: "CIS" },
      { facultyId: 4, name: "Faculty of Engineering", code: "ENG_ASU" },
    ],
  },
  {
    universityId: 3,
    name: "Alexandria University",
    city: "Alexandria",
    country: "Egypt",
    website: "https://alexu.edu.eg",
    logoUrl:
      "https://upload.wikimedia.org/wikipedia/ar/6/63/%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%A7%D9%84%D8%A5%D8%B3%D9%83%D9%86%D8%AF%D8%B1%D9%8A%D8%A9.png",
    faculties: [
      { facultyId: 5, name: "Faculty of Computing and Data Science", code: "CDS_ALEX" },
      { facultyId: 6, name: "Faculty of Engineering", code: "ENG_ALEX" },
    ],
  },
  {
    universityId: 4,
    name: "Mansoura University",
    city: "Mansoura",
    country: "Egypt",
    website: "https://mans.edu.eg",
    logoUrl:
      "https://upload.wikimedia.org/wikipedia/ar/5/5e/%D8%B4%D8%B9%D8%A7%D8%B1_%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%A7%D9%84%D9%85%D9%86%D8%B5%D9%88%D8%B1%D8%A9.png",
    faculties: [{ facultyId: 7, name: "Faculty of Computers and Information", code: "FCI_MANS" }],
  },
  {
    universityId: 5,
    name: "Assiut University",
    city: "Assiut",
    country: "Egypt",
    website: "https://aun.edu.eg",
    logoUrl: "https://www.aun.edu.eg/main/themes/gavias_colin/aun-logo.png",
    faculties: [{ facultyId: 9, name: "Faculty of Computers and Information", code: "FCI_AUN" }],
  },
  {
    universityId: 6,
    name: "Capital University",
    city: "Cairo",
    country: "Egypt",
    website: "https://capu.edu.eg",
    logoUrl: "https://upload.wikimedia.org/wikipedia/commons/c/c1/Capital_University_Logo.png",
    faculties: [
      { facultyId: 11, name: "Faculty of Computers and Artificial Intelligence", code: "FCAI_CAPU" },
      { facultyId: 12, name: "Faculty of Engineering", code: "ENG_CAPU" },
    ],
  },
  {
    universityId: 7,
    name: "Zagazig University",
    city: "Zagazig",
    country: "Egypt",
    website: "https://zu.edu.eg",
    logoUrl: "https://www.zu.edu.eg/uploads/2017/07/ZULogo.png",
    faculties: [{ facultyId: 13, name: "Faculty of Computers and Informatics", code: "FCI_ZU" }],
  },
  {
    universityId: 8,
    name: "Tanta University",
    city: "Tanta",
    country: "Egypt",
    website: "https://tanta.edu.eg",
    logoUrl: "https://tanta.edu.eg/tantaedu/img/unv-logo.png",
    faculties: [{ facultyId: 15, name: "Faculty of Engineering", code: "ENG_TANTA" }],
  },
  {
    universityId: 9,
    name: "Benha University",
    city: "Benha",
    country: "Egypt",
    website: "https://bu.edu.eg",
    logoUrl: "https://upload.wikimedia.org/wikipedia/en/d/dc/Benha_University_Logo.png",
    faculties: [{ facultyId: 17, name: "Faculty of Computers and AI", code: "FCAI_BU" }],
  },
  {
    universityId: 10,
    name: "Suez Canal University",
    city: "Ismailia",
    country: "Egypt",
    website: "https://suez.edu.eg",
    logoUrl:
      "https://suez.edu.eg/ar/wp-content/uploads/2022/01/266177708_300900118547240_420708396794634738_n-1.png",
    faculties: [{ facultyId: 18, name: "Faculty of Computers and Informatics", code: "FCI_SCU" }],
  },
  {
    universityId: 11,
    name: "German University in Cairo (GUC)",
    city: "New Cairo",
    country: "Egypt",
    website: "https://guc.edu.eg",
    logoUrl: "https://www.guc.edu.eg/img/guc_logo_og.png",
    faculties: [
      { facultyId: 19, name: "Faculty of Media Engineering and Technology", code: "MET_GUC" },
    ],
  },
  {
    universityId: 12,
    name: "The American University in Cairo (AUC)",
    city: "New Cairo",
    country: "Egypt",
    website: "https://aucegypt.edu",
    logoUrl: "https://upload.wikimedia.org/wikipedia/commons/1/1b/AUC_Arabic_Logo_2021.png",
    faculties: [{ facultyId: 21, name: "School of Sciences and Engineering", code: "SSE_AUC" }],
  },
  {
    universityId: 13,
    name: "Helwan University",
    city: "Cairo",
    country: "Egypt",
    website: "https://helwan.edu.eg",
    logoUrl: "https://upload.wikimedia.org/wikipedia/en/6/60/Helwan_University_logo.png",
    faculties: [
      { facultyId: 22, name: "Faculty of Engineering (Helwan)", code: "ENG_HU" },
      { facultyId: 23, name: "Faculty of Computers and Artificial Intelligence", code: "FCAI_HU" },
      { facultyId: 24, name: "Faculty of Commerce and Business Administration", code: "COM_HU" },
    ],
  },
];

export function useUniversityCatalog(enabled = true) {
  const query = useQuery({
    queryKey: queryKeys.universities.catalog,
    queryFn: ({ signal }) => universitiesApi.getCatalog(signal),
    staleTime: 5 * 60 * 1000,
    retry: false,
    enabled,
  });

  const universities = query.data && query.data.length > 0 ? query.data : FALLBACK_UNIVERSITIES;

  const logoByName = useMemo(() => {
    const map = new Map<string, string | null>();
    universities.forEach((uni) => {
      map.set(uni.name.toLowerCase(), uni.logoUrl ?? null);
    });
    return map;
  }, [universities]);

  const resolveLogo = (name: string): string | null =>
    logoByName.get(name.toLowerCase()) ??
    FALLBACK_UNIVERSITIES.find((u) => u.name.toLowerCase() === name.toLowerCase())?.logoUrl ??
    null;

  return { universities, resolveLogo, isLoading: query.isPending };
}
