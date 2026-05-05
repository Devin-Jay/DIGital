using System;

// different states of the analysis scene
    // final analysis is after exacvation and backfill
    // user submission is when the analysis scene opens from a user recording
    // answer key when shown from the collection form
public enum ArtifactAnalysisStart
{
    FinalAnalysis,
    UserSubmissionFromExcavation,
    AnswerKeyFromExcavation
}

public static class ArtifactAnalysisLaunchContext
{
    public static ArtifactAnalysisStart StartMode = ArtifactAnalysisStart.FinalAnalysis;
    
    public static ArtifactRecord UserSubmission;
    public static string ArtifactId;

    // see where analysis scene was launched
    // for Continue button visability
    public static bool LaunchedFromExcavation
    {
        get
        {
            return StartMode == ArtifactAnalysisStart.UserSubmissionFromExcavation ||
                   StartMode == ArtifactAnalysisStart.AnswerKeyFromExcavation;
        }
    }

    public static void Clear()
    {
        StartMode = ArtifactAnalysisStart.FinalAnalysis;
        UserSubmission = null;
        ArtifactId = null;
    }
}

// data
[Serializable]
public class ArtifactRecord
{
    public int id;
    public string date_discovered;
    public string investigator;
    public string area;
    public string unit;
    public string layer;
    public string site;
    public string associated_features;
    public string decorative_tech;
    public string material;
    public string firing;
    public string paint;
    public string cultural_affiliation;
    public string object_class;
    public string bag_number;
    public string artifact_id;
    public string created_at;
    public string updated_at;
    public string user_id;
}

[Serializable]
public class SingleArtifactResponse
{
    public bool ok;
    public string message;
    public ArtifactRecord artifact;
    public string error;
}

[Serializable]
public class ArtifactListResponse
{
    public bool ok;
    public ArtifactRecord[] artifacts;
    public string error;
}